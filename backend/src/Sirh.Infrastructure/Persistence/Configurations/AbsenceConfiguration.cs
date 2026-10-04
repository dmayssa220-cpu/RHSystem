using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sirh.Domain.TimeOff;

namespace Sirh.Infrastructure.Persistence.Configurations;

public sealed class AbsenceConfiguration : IEntityTypeConfiguration<Absence>
{
    public void Configure(EntityTypeBuilder<Absence> builder)
    {
        builder.Property(a => a.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(a => a.Comment).HasMaxLength(500);

        builder.HasIndex(a => a.TenantId);
        builder.HasIndex(a => new { a.EmployeeId, a.StartDate, a.EndDate });
    }
}
