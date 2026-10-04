namespace Sirh.Application.TimeOff;

public sealed record RequestAbsenceRequest(Guid EmployeeId, string Type, DateOnly StartDate, DateOnly EndDate, string? Comment);

public sealed record DecideAbsenceRequest(bool Approve, string? Comment);

public sealed record AbsenceSummary(
    Guid Id, Guid EmployeeId, string EmployeeFirstName, string EmployeeLastName,
    string Type, DateOnly StartDate, DateOnly EndDate, string Status, string? Comment);
