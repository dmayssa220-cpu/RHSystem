using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sirh.Domain.Personnel;

namespace Sirh.Infrastructure.Persistence.Configurations;

public sealed class ContractConfiguration : IEntityTypeConfiguration<Contract>
{
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.Property(c => c.Type).HasConversion<string>().HasMaxLength(20);
        // Dinar tunisien : 3 décimales (millimes). Voir README, section Conventions de données.
        builder.Property(c => c.BaseSalary).HasPrecision(18, 3);

        builder.HasIndex(c => c.TenantId);
        builder.HasIndex(c => c.EmployeeId);
    }
}
