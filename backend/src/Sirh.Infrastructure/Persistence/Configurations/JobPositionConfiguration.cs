using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sirh.Domain.Personnel;

namespace Sirh.Infrastructure.Persistence.Configurations;

public sealed class JobPositionConfiguration : IEntityTypeConfiguration<JobPosition>
{
    public void Configure(EntityTypeBuilder<JobPosition> builder)
    {
        builder.Property(j => j.Title).HasMaxLength(200).IsRequired();
        builder.HasIndex(j => j.TenantId);
    }
}
