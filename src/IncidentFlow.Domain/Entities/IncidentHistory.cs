using IncidentFlow.Domain.Common;
using IncidentFlow.Domain.Enums;

namespace IncidentFlow.Domain.Entities;

public class IncidentHistory : BaseEntity
{
    public Guid IncidentId { get; set; }
    public Incident Incident { get; set; } = null!;

    public string ChangedByUserId { get; set; } = string.Empty;
    public string ChangeType { get; set; } = string.Empty;
    public string? FieldName { get; set; }
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
    public IncidentStatus? OldStatus { get; set; }
    public IncidentStatus? NewStatus { get; set; }
    public DateTime ChangedAtUtc { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }
}
