using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sirh.Domain.Payroll;

namespace Sirh.Infrastructure.Persistence.Configurations;

public sealed class PayrollLegalParametersConfiguration : IEntityTypeConfiguration<PayrollLegalParameters>
{
    public void Configure(EntityTypeBuilder<PayrollLegalParameters> builder)
    {
        builder.Property(p => p.Source).HasMaxLength(300).IsRequired();

        builder.Property(p => p.CnssEmployeeRate).HasPrecision(9, 6);
        builder.Property(p => p.CnssEmployerRate).HasPrecision(9, 6);
        builder.Property(p => p.CnssCeilingAnnual).HasPrecision(18, 3);

        builder.Property(p => p.ProfessionalDeductionRate).HasPrecision(9, 6);
        builder.Property(p => p.ProfessionalDeductionCeilingAnnual).HasPrecision(18, 3);

        builder.Property(p => p.CssRate).HasPrecision(9, 6);

        builder.Property(p => p.FamilyDeductionHeadOfHousehold).HasPrecision(18, 3);
        builder.Property(p => p.FamilyDeductionPerChild).HasPrecision(18, 3);

        builder.Property(p => p.BracketsJson).HasColumnType("json");

        builder.HasIndex(p => p.EffectiveFrom);
    }
}
