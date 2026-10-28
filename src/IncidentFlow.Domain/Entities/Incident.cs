using IncidentFlow.Domain.Common;
using IncidentFlow.Domain.Enums;

namespace IncidentFlow.Domain.Entities;

public class Incident : AuditableEntity
{
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IncidentPriority Priority { get; set; } = IncidentPriority.Medium;
    public IncidentStatus Status { get; set; } = IncidentStatus.New;

    public Guid CategoryId { get; set; }
    public IncidentCategory Category { get; set; } = null!;

    public string CreatedByUserId { get; set; } = string.Empty;
    public string? AssignedTechnicianId { get; set; }

    public DateTime? ResolutionDateUtc { get; set; }
    public DateTime SlaDeadlineUtc { get; set; }
    public string? ResolutionNotes { get; set; }

    public ICollection<IncidentComment> Comments { get; set; } = new List<IncidentComment>();
    public ICollection<IncidentHistory> History { get; set; } = new List<IncidentHistory>();
    public ICollection<IncidentAssignment> Assignments { get; set; } = new List<IncidentAssignment>();
}
