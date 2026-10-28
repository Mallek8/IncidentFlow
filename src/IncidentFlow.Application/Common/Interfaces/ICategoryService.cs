using IncidentFlow.Application.Features.Incidents.Models;

namespace IncidentFlow.Application.Common.Interfaces;

public interface ICategoryService
{
    Task<IReadOnlyList<IncidentCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default);
    Task<IncidentCategoryDto> CreateCategoryAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
}
