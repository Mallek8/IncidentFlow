using IncidentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentFlow.Infrastructure.Persistence.Configurations;

public class IncidentHistoryConfiguration : IEntityTypeConfiguration<IncidentHistory>
{
    public void Configure(EntityTypeBuilder<IncidentHistory> builder)
    {
        builder.ToTable("IncidentHistories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ChangedByUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.ChangeType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.FieldName)
            .HasMaxLength(100);

        builder.Property(x => x.OldValue)
            .HasMaxLength(1000);

        builder.Property(x => x.NewValue)
            .HasMaxLength(1000);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.Property(x => x.OldStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.NewStatus)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => x.IncidentId);
        builder.HasIndex(x => x.ChangedAtUtc);
    }
}
