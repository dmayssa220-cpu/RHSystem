namespace Sirh.Application.Personnel;

public sealed record CreateContractRequest(
    Guid EmployeeId,
    string Type,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal BaseSalary,
    double? WeeklyHours);

public sealed record ContractSummary(
    Guid Id,
    Guid EmployeeId,
    string Type,
    DateOnly StartDate,
    DateOnly? EndDate,
    decimal BaseSalary);
