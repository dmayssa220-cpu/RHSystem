using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sirh.Domain.Compliance;

namespace Sirh.Infrastructure.Persistence.Configurations;

public sealed class AnomalyAlertConfiguration : IEntityTypeConfiguration<AnomalyAlert>
{
    public void Configure(EntityTypeBuilder<AnomalyAlert> builder)
    {
        builder.Property(a => a.RuleCode).HasMaxLength(100).IsRequired();
        builder.Property(a => a.Severity).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Message).HasMaxLength(1000);
        builder.Property(a => a.DecisionComment).HasMaxLength(500);

        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => a.Status);
        builder.HasIndex(a => a.PayslipId);
    }
}
