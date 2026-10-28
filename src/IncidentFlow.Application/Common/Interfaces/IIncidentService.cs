using IncidentFlow.Application.Common.Models;
using IncidentFlow.Application.Features.Incidents.Models;

namespace IncidentFlow.Application.Common.Interfaces;

/// <summary>
/// Incident lifecycle use-cases only (SRP). Categories and dashboard have dedicated services.
/// </summary>
public interface IIncidentService
{
    Task<IncidentDetailDto> CreateAsync(CreateIncidentRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<IncidentListItemDto>> GetPagedAsync(IncidentQuery query, CancellationToken cancellationToken = default);
    Task<IncidentDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IncidentDetailDto> UpdateStatusAsync(Guid id, UpdateIncidentStatusRequest request, CancellationToken cancellationToken = default);
    Task<IncidentDetailDto> AssignAsync(Guid id, AssignIncidentRequest request, CancellationToken cancellationToken = default);
    Task<IncidentCommentDto> AddCommentAsync(Guid id, AddCommentRequest request, CancellationToken cancellationToken = default);
    Task<IncidentDetailDto> ReopenAsync(Guid id, CancellationToken cancellationToken = default);
}
