using IncidentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentFlow.Infrastructure.Persistence.Configurations;

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incidents");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.TicketNumber)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.ResolutionNotes)
            .HasMaxLength(4000);

        builder.Property(x => x.CreatedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.AssignedTechnicianId)
            .HasMaxLength(450);

        builder.Property(x => x.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => x.TicketNumber)
            .IsUnique();

        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.Priority);
        builder.HasIndex(x => x.CreatedByUserId);
        builder.HasIndex(x => x.AssignedTechnicianId);
        builder.HasIndex(x => x.SlaDeadlineUtc);
        builder.HasIndex(x => new { x.Status, x.Priority });

        builder.HasMany(x => x.Comments)
            .WithOne(x => x.Incident)
            .HasForeignKey(x => x.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.History)
            .WithOne(x => x.Incident)
            .HasForeignKey(x => x.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Assignments)
            .WithOne(x => x.Incident)
            .HasForeignKey(x => x.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
