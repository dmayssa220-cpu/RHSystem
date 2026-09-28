using Microsoft.EntityFrameworkCore;
using Sirh.Application.Persistence;
using Sirh.Application.Security;
using Sirh.Domain.Personnel;

namespace Sirh.Application.Personnel;

public sealed class ContractService(IAppDbContext dbContext, ICurrentUserService currentUser)
{
    public async Task<IReadOnlyList<ContractSummary>> ListForEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default) =>
        await dbContext.Contracts
            .Where(c => c.EmployeeId == employeeId)
            .OrderByDescending(c => c.StartDate)
            .Select(c => new ContractSummary(c.Id, c.EmployeeId, c.Type.ToString(), c.StartDate, c.EndDate, c.BaseSalary))
            .ToListAsync(cancellationToken);

    public async Task<ContractSummary> CreateAsync(CreateContractRequest request, CancellationToken cancellationToken = default)
    {
        if (currentUser.TenantId is not { } tenantId)
        {
            throw new InvalidOperationException("Aucune société associée à l'utilisateur courant.");
        }

        var employeeExists = await dbContext.Employees.AnyAsync(e => e.Id == request.EmployeeId, cancellationToken);
        if (!employeeExists)
        {
            throw new InvalidOperationException("Salarié introuvable.");
        }

        var contract = new Contract
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = request.EmployeeId,
            Type = Enum.Parse<ContractType>(request.Type, ignoreCase: true),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            BaseSalary = request.BaseSalary,
            WeeklyHours = request.WeeklyHours,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Contracts.Add(contract);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ContractSummary(contract.Id, contract.EmployeeId, contract.Type.ToString(), contract.StartDate, contract.EndDate, contract.BaseSalary);
    }
}
