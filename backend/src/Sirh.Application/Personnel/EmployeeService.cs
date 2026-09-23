using Microsoft.EntityFrameworkCore;
using Sirh.Application.Persistence;
using Sirh.Application.Security;
using Sirh.Domain.Personnel;

namespace Sirh.Application.Personnel;

/// <summary>Cas d'usage du dossier salarié. La société (TenantId) vient toujours de l'utilisateur courant, jamais de la requête.</summary>
public sealed class EmployeeService(IAppDbContext dbContext, ICurrentUserService currentUser)
{
    public async Task<IReadOnlyList<EmployeeSummary>> ListAsync(CancellationToken cancellationToken = default) =>
        await dbContext.Employees
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .Select(e => new EmployeeSummary(e.Id, e.FirstName, e.LastName, e.Status.ToString(), e.HireDate))
            .ToListAsync(cancellationToken);

    public async Task<EmployeeDetail?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        return employee is null ? null : ToDetail(employee);
    }

    public async Task<EmployeeDetail> CreateAsync(CreateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        if (currentUser.TenantId is not { } tenantId)
        {
            throw new InvalidOperationException("Aucune société associée à l'utilisateur courant.");
        }

        var establishmentExists = await dbContext.Establishments
            .AnyAsync(e => e.Id == request.EstablishmentId, cancellationToken);
        if (!establishmentExists)
        {
            throw new InvalidOperationException("Établissement introuvable.");
        }

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EstablishmentId = request.EstablishmentId,
            DepartmentId = request.DepartmentId,
            JobPositionId = request.JobPositionId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Gender = Enum.Parse<Gender>(request.Gender, ignoreCase: true),
            DateOfBirth = request.DateOfBirth,
            NationalId = request.NationalId,
            PersonalEmail = request.PersonalEmail,
            PersonalPhone = request.PersonalPhone,
            HireDate = request.HireDate,
            Status = EmployeeStatus.Actif,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToDetail(employee);
    }

    public async Task<EmployeeDetail?> UpdateAsync(Guid id, UpdateEmployeeRequest request, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (employee is null)
        {
            return null;
        }

        employee.DepartmentId = request.DepartmentId;
        employee.JobPositionId = request.JobPositionId;
        employee.PersonalEmail = request.PersonalEmail;
        employee.PersonalPhone = request.PersonalPhone;
        employee.Status = Enum.Parse<EmployeeStatus>(request.Status, ignoreCase: true);
        employee.UpdatedAtUtc = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToDetail(employee);
    }

    private static EmployeeDetail ToDetail(Employee e) => new(
        e.Id, e.EstablishmentId, e.DepartmentId, e.JobPositionId,
        e.FirstName, e.LastName, e.Gender.ToString(), e.DateOfBirth,
        e.NationalId, e.PersonalEmail, e.PersonalPhone, e.HireDate, e.Status.ToString());
}
