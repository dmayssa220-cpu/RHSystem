namespace Sirh.Application.Personnel;

public sealed record CreateEmployeeRequest(
    Guid EstablishmentId,
    Guid? DepartmentId,
    Guid? JobPositionId,
    string FirstName,
    string LastName,
    string Gender,
    DateOnly DateOfBirth,
    string NationalId,
    string? PersonalEmail,
    string? PersonalPhone,
    DateOnly HireDate);

public sealed record UpdateEmployeeRequest(
    Guid? DepartmentId,
    Guid? JobPositionId,
    string? PersonalEmail,
    string? PersonalPhone,
    string Status);

public sealed record EmployeeSummary(
    Guid Id,
    string FirstName,
    string LastName,
    string Status,
    DateOnly HireDate);

public sealed record EmployeeDetail(
    Guid Id,
    Guid EstablishmentId,
    Guid? DepartmentId,
    Guid? JobPositionId,
    string FirstName,
    string LastName,
    string Gender,
    DateOnly DateOfBirth,
    string NationalId,
    string? PersonalEmail,
    string? PersonalPhone,
    DateOnly HireDate,
    string Status);
