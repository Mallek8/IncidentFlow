using IncidentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IncidentFlow.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Incident> Incidents { get; }
    DbSet<IncidentCategory> IncidentCategories { get; }
    DbSet<IncidentComment> IncidentComments { get; }
    DbSet<IncidentHistory> IncidentHistories { get; }
    DbSet<IncidentAssignment> IncidentAssignments { get; }
    DbSet<SlaPolicy> SlaPolicies { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}