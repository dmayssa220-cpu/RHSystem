using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sirh.Domain.Personnel;

namespace Sirh.Infrastructure.Persistence.Configurations;

public sealed class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.Property(e => e.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.LastName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.NationalId).HasMaxLength(50).IsRequired();
        builder.Property(e => e.PersonalEmail).HasMaxLength(200);
        builder.Property(e => e.PersonalPhone).HasMaxLength(30);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.Gender).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(e => e.TenantId);
        builder.HasIndex(e => e.EstablishmentId);
        // Un même numéro de CIN ne peut pas être utilisé deux fois dans la même société.
        builder.HasIndex(e => new { e.TenantId, e.NationalId }).IsUnique();
    }
}
