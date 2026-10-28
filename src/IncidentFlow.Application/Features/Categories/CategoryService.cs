using IncidentFlow.Application.Common.Exceptions;
using IncidentFlow.Application.Common.Interfaces;
using IncidentFlow.Application.Features.Incidents.Models;
using IncidentFlow.Application.Features.Incidents.Security;
using IncidentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentFlow.Application.Features.Categories;

public class CategoryService : ICategoryService
{
    private readonly IApplicationDbContext _db;
    private readonly IIncidentAccessPolicy _accessPolicy;

    public CategoryService(IApplicationDbContext db, IIncidentAccessPolicy accessPolicy)
    {
        _db = db;
        _accessPolicy = accessPolicy;
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

    public async Task<IncidentCategoryDto> CreateCategoryAsync(
        CreateCategoryRequest request,
        CancellationToken cancellationToken = default)
    {
        _accessPolicy.EnsureCanManageCategories();

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
}
