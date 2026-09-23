using Sirh.Domain.Common;

namespace Sirh.Domain.Personnel;

public sealed class Department : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EstablishmentId { get; set; }
    public string Name { get; set; } = string.Empty;
}
