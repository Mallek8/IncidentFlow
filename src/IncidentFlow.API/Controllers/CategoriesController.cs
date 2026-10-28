using IncidentFlow.Application.Common.Interfaces;
using IncidentFlow.Application.Common.Models;
using IncidentFlow.Application.Features.Incidents.Models;
using IncidentFlow.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IncidentFlow.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categories;

    public CategoriesController(ICategoryService categories)
    {
        _categories = categories;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<IncidentCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<IncidentCategoryDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await _categories.GetCategoriesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<IncidentCategoryDto>>.Ok(result));
    }

    [HttpPost]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IncidentCategoryDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<IncidentCategoryDto>>> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest(ApiResponse<IncidentCategoryDto>.Fail("Category name is required."));
        }

        var result = await _categories.CreateCategoryAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetAll), ApiResponse<IncidentCategoryDto>.Ok(result, "Category created."));
    }
}
