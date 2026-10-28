using IncidentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentFlow.Infrastructure.Persistence.Configurations;

public class SlaPolicyConfiguration : IEntityTypeConfiguration<SlaPolicy>
{
    public void Configure(EntityTypeBuilder<SlaPolicy> builder)
    {
        builder.ToTable("SlaPolicies");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Priority)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => x.Priority)
            .IsUnique()
            .HasFilter("[IsActive] = 1");
    }
}
