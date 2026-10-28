using IncidentFlow.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace IncidentFlow.Infrastructure.Persistence.Configurations;

public class IncidentCommentConfiguration : IEntityTypeConfiguration<IncidentComment>
{
    public void Configure(EntityTypeBuilder<IncidentComment> builder)
    {
        builder.ToTable("IncidentComments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.AuthorUserId)
            .IsRequired()
            .HasMaxLength(450);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasIndex(x => x.IncidentId);
        builder.HasIndex(x => x.AuthorUserId);
    }
}
