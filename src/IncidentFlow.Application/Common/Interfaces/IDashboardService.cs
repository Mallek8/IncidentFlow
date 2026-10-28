using IncidentFlow.Application.Features.Incidents.Models;

namespace IncidentFlow.Application.Common.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
