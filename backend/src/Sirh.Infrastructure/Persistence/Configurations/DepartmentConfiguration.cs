using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sirh.Domain.Personnel;

namespace Sirh.Infrastructure.Persistence.Configurations;

public sealed class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.Property(d => d.Name).HasMaxLength(200).IsRequired();
        builder.HasIndex(d => d.TenantId);
        builder.HasIndex(d => d.EstablishmentId);
    }
}
