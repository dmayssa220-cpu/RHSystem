namespace Sirh.Application.Payroll;

public sealed record PayrollPreviewRequest(Guid EmployeeId, bool? IsHeadOfHousehold = null, int? DependentChildren = null);

public sealed record PayrollTraceLineDto(string Label, decimal Amount, string? Detail);

public sealed record PayrollPreviewResult(
    Guid EmployeeId,
    decimal GrossMonthlySalary,
    decimal CnssEmployeeMonthly,
    decimal CnssEmployerMonthly,
    decimal TaxableAnnual,
    decimal IrppAnnual,
    decimal IrppMonthly,
    decimal CssMonthly,
    decimal NetMonthly,
    IReadOnlyList<PayrollTraceLineDto> Trace);

public sealed record CloseMonthRequest(int Year, int Month);

public sealed record PayslipSummary(
    Guid Id,
    Guid EmployeeId,
    string EmployeeFirstName,
    string EmployeeLastName,
    int PeriodYear,
    int PeriodMonth,
    decimal GrossMonthlySalary,
    decimal NetMonthly);

public sealed record CloseMonthResult(
    int Year,
    int Month,
    int EmployeesProcessed,
    int EmployeesSkippedNoContract,
    IReadOnlyList<PayslipSummary> Payslips);

public sealed record CnssQuarterlyDeclarationLine(
    Guid EmployeeId, string EmployeeFirstName, string EmployeeLastName,
    decimal GrossQuarterly, decimal CnssEmployeeQuarterly, decimal CnssEmployerQuarterly);

/// <summary>Déclaration Trimestrielle des Salaires (DTS) — loi n° 60-30, art. 46 : dépôt au plus tard le 15 du mois suivant le trimestre échu.</summary>
public sealed record CnssQuarterlyDeclaration(
    int Year, int Quarter, DateOnly DueDate,
    IReadOnlyList<CnssQuarterlyDeclarationLine> Lines,
    decimal TotalGross, decimal TotalCnssEmployee, decimal TotalCnssEmployer);

public sealed record WithholdingDeclarationLine(Guid EmployeeId, string EmployeeFirstName, string EmployeeLastName, decimal IrppMonthly, decimal CssMonthly);

/// <summary>Déclaration mensuelle de retenue à la source (IRPP + CSS) : dépôt au plus tard le 15 du mois suivant.</summary>
public sealed record WithholdingDeclaration(
    int Year, int Month, DateOnly DueDate,
    IReadOnlyList<WithholdingDeclarationLine> Lines,
    decimal TotalIrpp, decimal TotalCss);
