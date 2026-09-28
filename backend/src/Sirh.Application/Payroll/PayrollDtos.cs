namespace Sirh.Application.Payroll;

public sealed record PayrollPreviewRequest(Guid EmployeeId, bool IsHeadOfHousehold, int DependentChildren);

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
