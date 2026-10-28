using IncidentFlow.Application.Common.Exceptions;
using IncidentFlow.Application.Common.Interfaces;
using IncidentFlow.Application.Common.Models;
using IncidentFlow.Application.Features.Incidents.Mapping;
using IncidentFlow.Application.Features.Incidents.Models;
using IncidentFlow.Application.Features.Incidents.Security;
using IncidentFlow.Domain.Entities;
using IncidentFlow.Domain.Enums;
using IncidentFlow.Domain.Services;
using Microsoft.EntityFrameworkCore;

namespace IncidentFlow.Application.Features.Incidents.Services;

/// <summary>
/// Orchestrates incident lifecycle. Authorization, mapping, and ticket numbering are delegated.
/// </summary>
public class IncidentService : IIncidentService
{
    private readonly IApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _clock;
    private readonly IIncidentAccessPolicy _accessPolicy;
    private readonly IIncidentMapper _mapper;
    private readonly ITicketNumberGenerator _ticketNumbers;

    public IncidentService(
        IApplicationDbContext db,
        ICurrentUserService currentUser,
        IDateTimeProvider clock,
        IIncidentAccessPolicy accessPolicy,
        IIncidentMapper mapper,
        ITicketNumberGenerator ticketNumbers)
    {
        _db = db;
        _currentUser = currentUser;
        _clock = clock;
        _accessPolicy = accessPolicy;
        _mapper = mapper;
        _ticketNumbers = ticketNumbers;
    }

    public async Task<IncidentDetailDto> CreateAsync(CreateIncidentRequest request, CancellationToken cancellationToken = default)
    {
        _accessPolicy.EnsureAuthenticated();

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
            TicketNumber = await _ticketNumbers.GenerateAsync(cancellationToken),
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
        _accessPolicy.EnsureAuthenticated();
        var now = _clock.UtcNow;

        var incidents = _db.Incidents
            .AsNoTracking()
            .Include(i => i.Category)
            .AsQueryable();

        incidents = _accessPolicy.ApplyVisibilityFilter(incidents);

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

        return new PagedResult<IncidentListItemDto>
        {
            Items = pageItems.Select(i => _mapper.ToListItem(i, now)).ToList(),
            Page = page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<IncidentDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _accessPolicy.EnsureAuthenticated();

        var incident = await _db.Incidents
            .AsNoTracking()
            .Include(i => i.Category)
            .Include(i => i.Comments)
            .Include(i => i.History)
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), id);

        _accessPolicy.EnsureCanView(incident);
        return _mapper.ToDetail(incident, _clock.UtcNow, _accessPolicy.IsStaff());
    }

    public async Task<IncidentDetailDto> UpdateStatusAsync(
        Guid id,
        UpdateIncidentStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        _accessPolicy.EnsureAuthenticated();

        var incident = await LoadTrackedIncidentAsync(id, includeAssignments: false, cancellationToken);
        _accessPolicy.EnsureCanUpdateStatus(incident);

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

        if ((request.Status is IncidentStatus.Reopened or IncidentStatus.InProgress or IncidentStatus.Assigned)
            && (oldStatus is IncidentStatus.Resolved or IncidentStatus.Closed))
        {
            incident.ResolutionDateUtc = null;
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
        return _mapper.ToDetail(incident, _clock.UtcNow, includeInternalNotes: true);
    }

    public async Task<IncidentDetailDto> AssignAsync(
        Guid id,
        AssignIncidentRequest request,
        CancellationToken cancellationToken = default)
    {
        _accessPolicy.EnsureCanAssign();

        var incident = await LoadTrackedIncidentAsync(id, includeAssignments: true, cancellationToken);

        foreach (var active in incident.Assignments.Where(a => a.IsActive))
        {
            active.IsActive = false;
            active.UnassignedAtUtc = _clock.UtcNow;
        }

        incident.AssignedTechnicianId = request.TechnicianUserId;
        incident.UpdatedAtUtc = _clock.UtcNow;

        if (incident.Status is IncidentStatus.New or IncidentStatus.Reopened)
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
        return _mapper.ToDetail(incident, _clock.UtcNow, includeInternalNotes: true);
    }

    public async Task<IncidentCommentDto> AddCommentAsync(
        Guid id,
        AddCommentRequest request,
        CancellationToken cancellationToken = default)
    {
        _accessPolicy.EnsureAuthenticated();

        var incident = await _db.Incidents
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), id);

        _accessPolicy.EnsureCanView(incident);

        if (request.IsInternal && !_accessPolicy.IsStaff())
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

        return _mapper.ToComment(comment);
    }

    public async Task<IncidentDetailDto> ReopenAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var incident = await LoadTrackedIncidentAsync(id, includeAssignments: false, cancellationToken);
        _accessPolicy.EnsureCanReopen(incident);

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
        return _mapper.ToDetail(incident, _clock.UtcNow, _accessPolicy.IsStaff());
    }

    private async Task<Incident> LoadTrackedIncidentAsync(
        Guid id,
        bool includeAssignments,
        CancellationToken cancellationToken)
    {
        var query = _db.Incidents
            .Include(i => i.Category)
            .Include(i => i.Comments)
            .Include(i => i.History)
            .AsQueryable();

        if (includeAssignments)
        {
            query = query.Include(i => i.Assignments);
        }

        return await query.FirstOrDefaultAsync(i => i.Id == id, cancellationToken)
            ?? throw new NotFoundException(nameof(Incident), id);
    }
}