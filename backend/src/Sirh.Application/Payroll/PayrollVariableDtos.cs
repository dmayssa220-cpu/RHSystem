namespace Sirh.Application.Payroll;

public sealed record CreatePayrollVariableRequest(Guid EmployeeId, int PeriodYear, int PeriodMonth, string Type, string Label, decimal Amount);

public sealed record PayrollVariableSummary(Guid Id, Guid EmployeeId, int PeriodYear, int PeriodMonth, string Type, string Label, decimal Amount);
