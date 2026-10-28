using IncidentFlow.Application.Features.Incidents.Models;
using IncidentFlow.Domain.Entities;

namespace IncidentFlow.Application.Features.Incidents.Mapping;

/// <summary>
/// Single responsibility: map incident aggregates to API DTOs.
/// </summary>
public interface IIncidentMapper
{
    IncidentListItemDto ToListItem(Incident incident, DateTime utcNow);
    IncidentDetailDto ToDetail(Incident incident, DateTime utcNow, bool includeInternalNotes);
    IncidentCommentDto ToComment(IncidentComment comment);
}
