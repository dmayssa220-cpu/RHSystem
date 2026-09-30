using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sirh.Domain.Payroll;

namespace Sirh.Infrastructure.Persistence.Configurations;

public sealed class PayslipConfiguration : IEntityTypeConfiguration<Payslip>
{
    public void Configure(EntityTypeBuilder<Payslip> builder)
    {
        builder.Property(p => p.GrossMonthlySalary).HasPrecision(18, 3);
        builder.Property(p => p.CnssEmployeeMonthly).HasPrecision(18, 3);
        builder.Property(p => p.CnssEmployerMonthly).HasPrecision(18, 3);
        builder.Property(p => p.TaxableAnnual).HasPrecision(18, 3);
        builder.Property(p => p.IrppAnnual).HasPrecision(18, 3);
        builder.Property(p => p.IrppMonthly).HasPrecision(18, 3);
        builder.Property(p => p.CssMonthly).HasPrecision(18, 3);
        builder.Property(p => p.NetMonthly).HasPrecision(18, 3);
        builder.Property(p => p.TraceJson).HasColumnType("json");

        builder.HasIndex(p => p.TenantId);
        // Un salarié ne peut avoir qu'un seul bulletin clôturé par mois (idempotence de la clôture).
        builder.HasIndex(p => new { p.TenantId, p.EmployeeId, p.PeriodYear, p.PeriodMonth }).IsUnique();
    }
}
