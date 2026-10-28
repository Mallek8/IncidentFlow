using IncidentFlow.Domain.Common;
using IncidentFlow.Domain.Enums;

namespace IncidentFlow.Domain.Entities;

public class SlaPolicy : AuditableEntity
{
    public IncidentPriority Priority { get; set; }
    public int ResolutionTimeHours { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}
