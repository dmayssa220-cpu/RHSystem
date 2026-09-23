using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sirh.Domain.Personnel;

namespace Sirh.Infrastructure.Persistence.Configurations;

public sealed class EstablishmentConfiguration : IEntityTypeConfiguration<Establishment>
{
    public void Configure(EntityTypeBuilder<Establishment> builder)
    {
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.Property(e => e.Address).HasMaxLength(500);
        builder.HasIndex(e => e.TenantId);
    }
}
