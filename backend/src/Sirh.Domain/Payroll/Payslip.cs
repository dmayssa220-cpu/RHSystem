using Sirh.Domain.Common;

namespace Sirh.Domain.Payroll;

/// <summary>
/// Bulletin de paie clôturé pour un salarié, sur un mois donné : résultat figé du moteur de
/// paie au moment de la clôture. Immuable une fois créé (voir Sirh.Application.Payroll.PayrollService.CloseMonthAsync,
/// qui refuse de clôturer deux fois le même mois) — toute correction se fait par un rappel sur
/// un mois ultérieur, jamais par modification de ce bulletin.
/// </summary>
public sealed class Payslip : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }
    public Guid ContractId { get; set; }

    public int PeriodYear { get; set; }

    /// <summary>1 = janvier … 12 = décembre.</summary>
    public int PeriodMonth { get; set; }

    public decimal GrossMonthlySalary { get; set; }
    public decimal CnssEmployeeMonthly { get; set; }
    public decimal CnssEmployerMonthly { get; set; }
    public decimal TaxableAnnual { get; set; }
    public decimal IrppAnnual { get; set; }
    public decimal IrppMonthly { get; set; }
    public decimal CssMonthly { get; set; }
    public decimal NetMonthly { get; set; }

    public bool IsHeadOfHousehold { get; set; }
    public int DependentChildren { get; set; }

    /// <summary>Trace de calcul complète au moment de la clôture (JSON), pour pouvoir l'expliquer plus tard sans recalcul.</summary>
    public string TraceJson { get; set; } = "[]";

    public DateTime CreatedAtUtc { get; set; }
}
