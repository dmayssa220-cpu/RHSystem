using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sirh.Domain.Payroll;

namespace Sirh.Infrastructure.Persistence.Configurations;

public sealed class PayrollVariableConfiguration : IEntityTypeConfiguration<PayrollVariable>
{
    public void Configure(EntityTypeBuilder<PayrollVariable> builder)
    {
        builder.Property(v => v.Type).HasConversion<string>().HasMaxLength(30);
        builder.Property(v => v.Label).HasMaxLength(200).IsRequired();
        builder.Property(v => v.Amount).HasPrecision(18, 3);

        builder.HasIndex(v => v.TenantId);
        builder.HasIndex(v => new { v.EmployeeId, v.PeriodYear, v.PeriodMonth });
    }
}
