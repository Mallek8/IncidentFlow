using IncidentFlow.Application.Common.Interfaces;
using IncidentFlow.Application.Features.Incidents.Models;
using IncidentFlow.Application.Features.Incidents.Security;
using IncidentFlow.Domain.Enums;
using IncidentFlow.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace IncidentFlow.Application.Features.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly IApplicationDbContext _db;
    private readonly IIncidentAccessPolicy _accessPolicy;

    public DashboardService(IApplicationDbContext db, IIncidentAccessPolicy accessPolicy)
    {
        _db = db;
        _accessPolicy = accessPolicy;
    }

    public async Task<DashboardSummaryDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        _accessPolicy.EnsureAuthenticated();

        var query = _accessPolicy.ApplyVisibilityFilter(_db.Incidents.AsNoTracking());
        var incidents = await query.Include(i => i.Category).ToListAsync(cancellationToken);

        var openStatuses = new[]
        {
            IncidentStatus.New,
            IncidentStatus.Assigned,
            IncidentStatus.InProgress,
            IncidentStatus.Reopened
        };

        var resolved = incidents.Where(i => i.Status is IncidentStatus.Resolved or IncidentStatus.Closed).ToList();
        var resolvedWithDuration = resolved
            .Where(i => i.ResolutionDateUtc.HasValue)
            .Select(i => SlaCalculator.CalculateResolutionHours(i.CreatedAtUtc, i.ResolutionDateUtc)!.Value)
            .ToList();

        var slaCompliant = resolved.Count(i =>
            i.ResolutionDateUtc.HasValue && i.ResolutionDateUtc.Value <= i.SlaDeadlineUtc);

        var byCategory = incidents
            .GroupBy(i => i.Category.Name)
            .Select(g => new NamedCountDto(g.Key, g.Count()))
            .OrderByDescending(x => x.Count)
            .ToList();

        var byPriority = incidents
            .GroupBy(i => i.Priority.ToString())
            .Select(g => new NamedCountDto(g.Key, g.Count()))
            .ToList();

        var monthly = incidents
            .GroupBy(i => new { i.CreatedAtUtc.Year, i.CreatedAtUtc.Month })
            .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
            .TakeLast(6)
            .Select(g => new MonthlyTrendDto($"{g.Key.Year}-{g.Key.Month:D2}", g.Count()))
            .ToList();

        return new DashboardSummaryDto(
            incidents.Count,
            incidents.Count(i => openStatuses.Contains(i.Status)),
            incidents.Count(i => i.Priority == IncidentPriority.Critical && openStatuses.Contains(i.Status)),
            resolved.Count,
            resolvedWithDuration.Count == 0 ? null : resolvedWithDuration.Average(),
            resolved.Count == 0 ? 100 : Math.Round(slaCompliant * 100.0 / resolved.Count, 1),
            byCategory,
            byPriority,
            monthly);
    }
}