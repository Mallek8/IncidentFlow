using IncidentFlow.Application.Features.Incidents.Models;
using IncidentFlow.Domain.Entities;
using IncidentFlow.Domain.Services;

namespace IncidentFlow.Application.Features.Incidents.Mapping;

public class IncidentMapper : IIncidentMapper
{
    public IncidentListItemDto ToListItem(Incident incident, DateTime utcNow) =>
        new(
            incident.Id,
            incident.TicketNumber,
            incident.Title,
            incident.Category.Name,
            incident.Priority,
            incident.Status,
            incident.CreatedByUserId,
            incident.AssignedTechnicianId,
            incident.CreatedAtUtc,
            incident.UpdatedAtUtc,
            incident.SlaDeadlineUtc,
            SlaCalculator.IsOverdue(incident.SlaDeadlineUtc, incident.ResolutionDateUtc, utcNow));

    public IncidentDetailDto ToDetail(Incident incident, DateTime utcNow, bool includeInternalNotes)
    {
        var comments = incident.Comments
            .Where(c => includeInternalNotes || !c.IsInternal)
            .OrderBy(c => c.CreatedAtUtc)
            .Select(ToComment)
            .ToList();

        var history = incident.History
            .OrderBy(h => h.ChangedAtUtc)
            .Select(h => new IncidentHistoryDto(
                h.Id,
                h.ChangedByUserId,
                h.ChangeType,
                h.FieldName,
                h.OldValue,
                h.NewValue,
                h.OldStatus,
                h.NewStatus,
                h.ChangedAtUtc,
                h.Notes))
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
            SlaCalculator.IsOverdue(incident.SlaDeadlineUtc, incident.ResolutionDateUtc, utcNow),
            IncidentStatusTransitionRules.GetAllowedTargets(incident.Status).ToList(),
            comments,
            history);
    }

    public IncidentCommentDto ToComment(IncidentComment comment) =>
        new(comment.Id, comment.AuthorUserId, comment.Content, comment.IsInternal, comment.CreatedAtUtc);
}
