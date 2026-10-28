using IncidentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentFlow.Infrastructure.Persistence.Configurations;

public class IncidentAssignmentConfiguration : IEntityTypeConfiguration<IncidentAssignment>
{
    public void Configure(EntityTypeBuilder<IncidentAssignment> builder)
    {
        builder.ToTable("IncidentAssignments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TechnicianUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.AssignedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.IncidentId);
        builder.HasIndex(x => x.TechnicianUserId);
        builder.HasIndex(x => new { x.IncidentId, x.IsActive });
    }
}
