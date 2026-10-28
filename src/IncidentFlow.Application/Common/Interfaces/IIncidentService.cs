using IncidentFlow.Application.Common.Models;
using IncidentFlow.Application.Features.Incidents.Models;

namespace IncidentFlow.Application.Common.Interfaces;

public interface IIncidentService
{
    Task<IncidentDetailDto> CreateAsync(CreateIncidentRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<IncidentListItemDto>> GetPagedAsync(IncidentQuery query, CancellationToken cancellationToken = default);
    Task<IncidentDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IncidentDetailDto> UpdateStatusAsync(Guid id, UpdateIncidentStatusRequest request, CancellationToken cancellationToken = default);
    Task<IncidentDetailDto> AssignAsync(Guid id, AssignIncidentRequest request, CancellationToken cancellationToken = default);
    Task<IncidentCommentDto> AddCommentAsync(Guid id, AddCommentRequest request, CancellationToken cancellationToken = default);
    Task<IncidentDetailDto> ReopenAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IncidentCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IncidentCategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<DashboardSummaryDto> GetDashboardAsync(CancellationToken cancellationToken = default);
}
