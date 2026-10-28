using IncidentFlow.Application.Common.Exceptions;
using IncidentFlow.Application.Common.Interfaces;
using IncidentFlow.Application.Common.Models;
using IncidentFlow.Application.Features.Incidents.Models;
using IncidentFlow.Domain.Constants;
using IncidentFlow.Domain.Entities;
using IncidentFlow.Domain.Enums;
using IncidentFlow.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace IncidentFlow.Application.Features.Incidents.Services;

public class IncidentService : IIncidentService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;

    public IncidentService(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<IncidentDetailDto> CreateAsync(CreateIncidentRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var category = await _db.IncidentCategories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && c.IsActive, cancellationToken)
            ?? throw new NotFoundException(nameof(IncidentCategory), request.CategoryId);

        var slaHours = await _db.SlaPolicies
            .Where(p => p.Priority == request.Priority && p.IsActive)
            .Select(p => (int?)p.ResolutionTimeHours)
            .FirstOrDefaultAsync(cancellationToken);

        var createdAt = _clock.UtcNow;
        var incident = new Incident
        {
            TicketNumber = await GenerateTicketNumberAsync(cancellationToken),
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CategoryId = category.Id,
            Priority = request.Priority,
            Status = IncidentStatus.New,
            CreatedByUserId = _currentUser.UserId!,
            CreatedAtUtc = createdAt,
            SlaDeadlineUtc = SlaCalculator.CalculateDeadlineUtc(createdAt, request.Priority, slaHours)
        };

        incident.History.Add(new IncidentHistory
        {
            ChangedByUserId = _currentUser.UserId!,
            ChangeType = "Created",
            NewStatus = IncidentStatus.New,
            ChangedAtUtc = createdAt,
            Notes = "Incident created."
        });

        _db.Incidents.Add(incident);
        await _db.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(incident.Id, cancellationToken);
    }

    public async Task<PagedResult<IncidentListItemDto>> GetPagedAsync(IncidentQuery query, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        var now = _clock.UtcNow;

        var incidents = _db.Incidents
            .AsNoTracking()
            .Include(i => i.Category)
            .AsQueryable();

        incidents = ApplyVisibilityFilter(incidents);

        if (query.CreatedByMe)
        {
            incidents = incidents.Where(i => i.CreatedByUserId == _currentUser.UserId);
        }

        if (query.AssignedToMe)
        {
            incidents = incidents.Where(i => i.AssignedTechnicianId == _currentUser.UserId);
        }

        if (query.Status.HasValue)
        {
            incidents = incidents.Where(i => i.Status == query.Status);
        }

        if (query.Priority.HasValue)
        {
            incidents = incidents.Where(i => i.Priority == query.Priority);
        }

        if (query.CategoryId.HasValue)
        {
            incidents = incidents.Where(i => i.CategoryId == query.CategoryId);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            incidents = incidents.Where(i =>
                i.Title.Contains(term) ||
                i.TicketNumber.Contains(term) ||
                i.Description.Contains(term));
        }

        if (query.OverdueOnly == true)
        {
            incidents = incidents.Where(i =>
                i.Status != IncidentStatus.Resolved &&
                i.Status != IncidentStatus.Closed &&
                i.SlaDeadlineUtc < now);
        }

        incidents = (query.SortBy?.ToLowerInvariant()) switch
        {
            "title" => query.SortDescending ? incidents.OrderByDescending(i => i.Title) : incidents.OrderBy(i => i.Title),
            "priority" => query.SortDescending ? incidents.OrderByDescending(i => i.Priority) : incidents.OrderBy(i => i.Priority),
            "status" => query.SortDescending ? incidents.OrderByDescending(i => i.Status) : incidents.OrderBy(i => i.Status),
            "sla" => query.SortDescending ? incidents.OrderByDescending(i => i.SlaDeadlineUtc) : incidents.OrderBy(i => i.SlaDeadlineUtc),
            _ => query.SortDescending ? incidents.OrderByDescending(i => i.CreatedAtUtc) : incidents.OrderBy(i => i.CreatedAtUtc)
        };

        var total = await incidents.CountAsync(cancellationToken);
        var page = Math.Max(query.Page, 1);
        var pageItems = await incidents
            .Skip((page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var items = pageItems.Select(i => new IncidentListItemDto(
                i.Id,
                i.TicketNumber,
                i.Title,
                i.Category.Name,
                i.Priority,
                i.Status,
                i.CreatedByUserId,
                i.AssignedTechnicianId,
                i.CreatedAtUtc,
                i.UpdatedAtUtc,
                i.SlaDeadlineUtc,
                SlaCalculator.IsOverdue(i.SlaDeadlineUtc, i.ResolutionDateUtc, now)))
            .ToList();

        return new PagedResult<IncidentListItemDto>
        {
            Items = items,
            Page = page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<IncidentDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var incident = await _db.Incidents
            .AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Comments)
            .Include(i => i.History)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), id);

        EnsureCanView(incident);
        return MapDetail(incident);
    }

    public async Task<IncidentDetailDto> UpdateStatusAsync(Guid id, UpdateIncidentStatusRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        EnsureStaff();

        var incident = await _db.Incidents
            .Include(i => i.Category)
            .Include(i => i.Comments)
            .Include(i => i.History)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), id);

        if (_currentUser.IsInRole(Roles.Technician) &&
            !_currentUser.IsInRole(Roles.Admin) &&
            incident.AssignedTechnicianId != _currentUser.UserId)
        {
            throw new ForbiddenAccessException("Technicians can only update incidents assigned to them.");
        }

        if (!IncidentStatusTransitionRules.CanTransition(incident.Status, request.Status))
        {
            throw new BusinessRuleException(
                $"Transition from {incident.Status} to {request.Status} is not allowed.");
        }

        var oldStatus = incident.Status;
        incident.Status = request.Status;
        incident.UpdatedAtUtc = _clock.UtcNow;

        if (request.Status == IncidentStatus.Resolved)
        {
            incident.ResolutionDateUtc = _clock.UtcNow;
            incident.ResolutionNotes = request.ResolutionNotes?.Trim();
        }

        if (request.Status is IncidentStatus.Reopened or IncidentStatus.InProgress or IncidentStatus.Assigned)
        {
            if (oldStatus is IncidentStatus.Resolved or IncidentStatus.Closed)
            {
                incident.ResolutionDateUtc = null;
            }
        }

        incident.History.Add(new IncidentHistory
        {
            ChangedByUserId = _currentUser.UserId!,
            ChangeType = "StatusChanged",
            OldStatus = oldStatus,
            NewStatus = request.Status,
            ChangedAtUtc = _clock.UtcNow,
            Notes = request.ResolutionNotes
        });

        await _db.SaveChangesAsync(cancellationToken);
        return MapDetail(incident);
    }

    public async Task<IncidentDetailDto> AssignAsync(Guid id, AssignIncidentRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        if (!_currentUser.IsInRole(Roles.Admin))
        {
            throw new ForbiddenAccessException("Only administrators can assign incidents.");
        }

        var incident = await _db.Incidents
            .Include(i => i.Category)
            .Include(i => i.Comments)
            .Include(i => i.History)
            .Include(i => i.Assignments)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), id);

        foreach (var active in incident.Assignments.Where(a => a.IsActive))
        {
            active.IsActive = false;
            active.UnassignedAtUtc = _clock.UtcNow;
        }

        incident.AssignedTechnicianId = request.TechnicianUserId;
        incident.UpdatedAtUtc = _clock.UtcNow;

        if (incident.Status == IncidentStatus.New || incident.Status == IncidentStatus.Reopened)
        {
            var old = incident.Status;
            incident.Status = IncidentStatus.Assigned;
            incident.History.Add(new IncidentHistory
            {
                ChangedByUserId = _currentUser.UserId!,
                ChangeType = "StatusChanged",
                OldStatus = old,
                NewStatus = IncidentStatus.Assigned,
                ChangedAtUtc = _clock.UtcNow,
                Notes = "Auto-transitioned on assignment."
            });
        }

        incident.Assignments.Add(new IncidentAssignment
        {
            TechnicianUserId = request.TechnicianUserId,
            AssignedByUserId = _currentUser.UserId!,
            AssignedAtUtc = _clock.UtcNow,
            IsActive = true,
            Notes = request.Notes
        });

        incident.History.Add(new IncidentHistory
        {
            ChangedByUserId = _currentUser.UserId!,
            ChangeType = "Assigned",
            FieldName = nameof(Incident.AssignedTechnicianId),
            NewValue = request.TechnicianUserId,
            ChangedAtUtc = _clock.UtcNow,
            Notes = request.Notes
        });

        await _db.SaveChangesAsync(cancellationToken);
        return MapDetail(incident);
    }

    public async Task<IncidentCommentDto> AddCommentAsync(Guid id, AddCommentRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var incident = await _db.Incidents
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), id);

        EnsureCanView(incident);

        if (request.IsInternal && !IsStaff())
        {
            throw new ForbiddenAccessException("Only technicians and admins can add internal notes.");
        }

        var comment = new IncidentComment
        {
            IncidentId = incident.Id,
            AuthorUserId = _currentUser.UserId!,
            Content = request.Content.Trim(),
            IsInternal = request.IsInternal,
            CreatedAtUtc = _clock.UtcNow
        };

        _db.IncidentComments.Add(comment);
        incident.UpdatedAtUtc = _clock.UtcNow;
        await _db.SaveChangesAsync(cancellationToken);

        return new IncidentCommentDto(
            comment.Id,
            comment.AuthorUserId,
            comment.Content,
            comment.IsInternal,
            comment.CreatedAtUtc);
    }

    public async Task<IncidentDetailDto> ReopenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();

        var incident = await _db.Incidents
            .Include(i => i.Category)
            .Include(i => i.Comments)
            .Include(i => i.History)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), id);

        var isOwner = incident.CreatedByUserId == _currentUser.UserId;
        if (!isOwner && !_currentUser.IsInRole(Roles.Admin))
        {
            throw new ForbiddenAccessException("Only the reporter or an admin can reopen an incident.");
        }

        if (!IncidentStatusTransitionRules.CanTransition(incident.Status, IncidentStatus.Reopened))
        {
            throw new BusinessRuleException($"Incident in status {incident.Status} cannot be reopened.");
        }

        var old = incident.Status;
        incident.Status = IncidentStatus.Reopened;
        incident.ResolutionDateUtc = null;
        incident.UpdatedAtUtc = _clock.UtcNow;
        incident.History.Add(new IncidentHistory
        {
            ChangedByUserId = _currentUser.UserId!,
            ChangeType = "StatusChanged",
            OldStatus = old,
            NewStatus = IncidentStatus.Reopened,
            ChangedAtUtc = _clock.UtcNow,
            Notes = "Incident reopened."
        });

        await _db.SaveChangesAsync(cancellationToken);
        return MapDetail(incident);
    }

    public async Task<IReadOnlyList<IncidentCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        return await _db.IncidentCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new IncidentCategoryDto(c.Id, c.Name, c.Description, c.IsActive))
            .ToListAsync(cancellationToken);
    }

    public async Task<IncidentCategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        if (!_currentUser.IsInRole(Roles.Admin))
        {
            throw new ForbiddenAccessException("Only administrators can manage categories.");
        }

        var name = request.Name.Trim();
        var exists = await _db.IncidentCategories.AnyAsync(c => c.Name == name, cancellationToken);
        if (exists)
        {
            throw new BusinessRuleException($"Category '{name}' already exists.");
        }

        var category = new IncidentCategory
        {
            Name = name,
            Description = request.Description?.Trim(),
            IsActive = true
        };

        _db.IncidentCategories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);

        return new IncidentCategoryDto(category.Id, category.Name, category.Description, category.IsActive);
    }

    public async Task<DashboardSummaryDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        EnsureAuthenticated();
        var now = _clock.UtcNow;

        var query = ApplyVisibilityFilter(_db.Incidents.AsNoTracking());

        var incidents = await query
            .Include(i => i.Category)
            .ToListAsync(cancellationToken);

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

    private IQueryable<Incident> ApplyVisibilityFilter(IQueryable<Incident> query)
    {
        if (_currentUser.IsInRole(Roles.Admin))
        {
            return query;
        }

        if (_currentUser.IsInRole(Roles.Technician))
        {
            return query.Where(i =>
                i.AssignedTechnicianId == _currentUser.UserId ||
                i.CreatedByUserId == _currentUser.UserId ||
                i.Status == IncidentStatus.New);
        }

        return query.Where(i => i.CreatedByUserId == _currentUser.UserId);
    }

    private void EnsureCanView(Incident incident)
    {
        if (_currentUser.IsInRole(Roles.Admin))
        {
            return;
        }

        if (_currentUser.IsInRole(Roles.Technician))
        {
            if (incident.AssignedTechnicianId == _currentUser.UserId ||
                incident.CreatedByUserId == _currentUser.UserId ||
                incident.Status == IncidentStatus.New)
            {
                return;
            }
        }

        if (incident.CreatedByUserId == _currentUser.UserId)
        {
            return;
        }

        throw new ForbiddenAccessException("You are not allowed to view this incident.");
    }

    private IncidentDetailDto MapDetail(Incident incident)
    {
        var now = _clock.UtcNow;
        var showInternal = IsStaff();

        var comments = incident.Comments
            .Where(c => showInternal || !c.IsInternal)
            .OrderBy(c => c.CreatedAtUtc)
            .Select(c => new IncidentCommentDto(
                c.Id, c.AuthorUserId, c.Content, c.IsInternal, c.CreatedAtUtc))
            .ToList();

        var history = incident.History
            .OrderBy(h => h.ChangedAtUtc)
            .Select(h => new IncidentHistoryDto(
                h.Id, h.ChangedByUserId, h.ChangeType, h.FieldName, h.OldValue, h.NewValue,
                h.OldStatus, h.NewStatus, h.ChangedAtUtc, h.Notes))
            .ToList();

        return new IncidentDetailDto(
            incident.Id,
            incident.TicketNumber,
            incident.Title,
            incident.Description,
            incident.CategoryId,
            incident.Category?.Name ?? string.Empty,
            incident.Priority,
            incident.Status,
            incident.CreatedByUserId,
            incident.AssignedTechnicianId,
            incident.CreatedAtUtc,
            incident.UpdatedAtUtc,
            incident.ResolutionDateUtc,
            incident.SlaDeadlineUtc,
            incident.ResolutionNotes,
            SlaCalculator.IsOverdue(incident.SlaDeadlineUtc, incident.ResolutionDateUtc, now),
            IncidentStatusTransitionRules.GetAllowedTargets(incident.Status).ToList(),
            comments,
            history);
    }

    private async Task<string> GenerateTicketNumberAsync(CancellationToken cancellationToken)
    {
        var count = await _db.Incidents.CountAsync(cancellationToken);
        return $"INC-{100001 + count}";
    }

    private void EnsureAuthenticated()
    {
        if (!_currentUser.IsAuthenticated || string.IsNullOrWhiteSpace(_currentUser.UserId))
        {
            throw new UnauthorizedAccessException("Authentication required.");
        }
    }

    private void EnsureStaff()
    {
        if (!IsStaff())
        {
            throw new ForbiddenAccessException("Only technicians and admins can perform this action.");
        }
    }

    private bool IsStaff() =>
        _currentUser.IsInRole(Roles.Admin) || _currentUser.IsInRole(Roles.Technician);
}