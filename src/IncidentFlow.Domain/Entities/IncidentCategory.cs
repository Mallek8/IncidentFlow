using IncidentFlow.Domain.Common;

namespace IncidentFlow.Domain.Entities;

public class IncidentCategory : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
