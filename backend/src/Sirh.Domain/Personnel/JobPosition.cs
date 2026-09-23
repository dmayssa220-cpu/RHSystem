using Sirh.Domain.Common;

namespace Sirh.Domain.Personnel;

public sealed class JobPosition : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Title { get; set; } = string.Empty;
}
