namespace Sirh.Application.Compliance;

public sealed record AnomalyAlertSummary(
    Guid Id, Guid PayslipId, Guid EmployeeId, string EmployeeFirstName, string EmployeeLastName,
    int PeriodYear, int PeriodMonth, string RuleCode, string Severity, string Message, string Status, string? DecisionComment);

public sealed record DecideAnomalyRequest(bool Confirm, string? Comment);
