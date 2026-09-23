using Sirh.Domain.Common;

namespace Sirh.Domain.Personnel;

/// <summary>Contrat de travail d'un salarié. Un salarié peut avoir plusieurs contrats dans le temps (CDD renouvelé, puis CDI…).</summary>
public sealed class Contract : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }

    public ContractType Type { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }

    /// <summary>Salaire de base mensuel, en dinar tunisien (3 décimales : le dinar se divise en 1 000 millimes).</summary>
    public decimal BaseSalary { get; set; }

    public double? WeeklyHours { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
