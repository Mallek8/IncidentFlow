using FluentValidation;
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
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _incidents;
    private readonly IValidator<CreateIncidentRequest> _createValidator;
    private readonly IValidator<UpdateIncidentStatusRequest> _statusValidator;
    private readonly IValidator<AssignIncidentRequest> _assignValidator;
    private readonly IValidator<AddCommentRequest> _commentValidator;

    public IncidentsController(
        IIncidentService incidents,
        IValidator<CreateIncidentRequest> createValidator,
        IValidator<UpdateIncidentStatusRequest> statusValidator,
        IValidator<AssignIncidentRequest> assignValidator,
        IValidator<AddCommentRequest> commentValidator)
    {
        _incidents = incidents;
        _createValidator = createValidator;
        _statusValidator = statusValidator;
        _assignValidator = assignValidator;
        _commentValidator = commentValidator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<IncidentListItemDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<PagedResult<IncidentListItemDto>>>> GetPaged(
        [FromQuery] IncidentQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _incidents.GetPagedAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<IncidentListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<IncidentDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IncidentDetailDto>>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _incidents.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<IncidentDetailDto>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<IncidentDetailDto>), StatusCodes.Status201Created)]
    public async Task<ActionResult<ApiResponse<IncidentDetailDto>>> Create(
        [FromBody] CreateIncidentRequest request,
        CancellationToken cancellationToken)
    {
        await _createValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _incidents.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<IncidentDetailDto>.Ok(result, "Incident created."));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Roles = $"{Roles.Admin},{Roles.Technician}")]
    [ProducesResponseType(typeof(ApiResponse<IncidentDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IncidentDetailDto>>> UpdateStatus(
        Guid id,
        [FromBody] UpdateIncidentStatusRequest request,
        CancellationToken cancellationToken)
    {
        await _statusValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _incidents.UpdateStatusAsync(id, request, cancellationToken);
        return Ok(ApiResponse<IncidentDetailDto>.Ok(result, "Status updated."));
    }

    [HttpPost("{id:guid}/assign")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(ApiResponse<IncidentDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IncidentDetailDto>>> Assign(
        Guid id,
        [FromBody] AssignIncidentRequest request,
        CancellationToken cancellationToken)
    {
        await _assignValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _incidents.AssignAsync(id, request, cancellationToken);
        return Ok(ApiResponse<IncidentDetailDto>.Ok(result, "Incident assigned."));
    }

    [HttpPost("{id:guid}/comments")]
    [ProducesResponseType(typeof(ApiResponse<IncidentCommentDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IncidentCommentDto>>> AddComment(
        Guid id,
        [FromBody] AddCommentRequest request,
        CancellationToken cancellationToken)
    {
        await _commentValidator.ValidateAndThrowAsync(request, cancellationToken);
        var result = await _incidents.AddCommentAsync(id, request, cancellationToken);
        return Ok(ApiResponse<IncidentCommentDto>.Ok(result, "Comment added."));
    }

    [HttpPost("{id:guid}/reopen")]
    [ProducesResponseType(typeof(ApiResponse<IncidentDetailDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<IncidentDetailDto>>> Reopen(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await _incidents.ReopenAsync(id, cancellationToken);
        return Ok(ApiResponse<IncidentDetailDto>.Ok(result, "Incident reopened."));
    }
}
