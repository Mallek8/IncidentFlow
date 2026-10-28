using IncidentFlow.Domain.Common;

namespace IncidentFlow.Domain.Entities;

public class IncidentAssignment : AuditableEntity
{
    public Guid IncidentId { get; set; }
    public Incident Incident { get; set; } = null!;

    public string TechnicianUserId { get; set; } = string.Empty;
    public string AssignedByUserId { get; set; } = string.Empty;
    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UnassignedAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Notes { get; set; }
}