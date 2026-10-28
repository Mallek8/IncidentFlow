using IncidentFlow.Domain.Common;

namespace IncidentFlow.Domain.Entities;

public class IncidentComment : AuditableEntity
{
    public Guid IncidentId { get; set; }
    public Incident Incident { get; set; } = null!;

    public string AuthorUserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Internal notes are visible to technicians and admins only.
    /// </summary>
    public bool IsInternal { get; set; }
}
