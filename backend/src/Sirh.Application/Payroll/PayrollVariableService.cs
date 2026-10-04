using Microsoft.EntityFrameworkCore;
using Sirh.Application.Persistence;
using Sirh.Application.Security;
using Sirh.Domain.Payroll;

namespace Sirh.Application.Payroll;

public sealed class PayrollVariableService(IAppDbContext dbContext, ICurrentUserService currentUser)
{
    public async Task<IReadOnlyList<PayrollVariableSummary>> ListAsync(Guid employeeId, int year, int month, CancellationToken cancellationToken = default) =>
        await dbContext.PayrollVariables
            .Where(v => v.EmployeeId == employeeId && v.PeriodYear == year && v.PeriodMonth == month)
            .OrderBy(v => v.CreatedAtUtc)
            .Select(v => new PayrollVariableSummary(v.Id, v.EmployeeId, v.PeriodYear, v.PeriodMonth, v.Type.ToString(), v.Label, v.Amount))
            .ToListAsync(cancellationToken);

    public async Task<PayrollVariableSummary> CreateAsync(CreatePayrollVariableRequest request, CancellationToken cancellationToken = default)
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

        var variable = new PayrollVariable
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = request.EmployeeId,
            PeriodYear = request.PeriodYear,
            PeriodMonth = request.PeriodMonth,
            Type = Enum.Parse<PayrollVariableType>(request.Type, ignoreCase: true),
            Label = request.Label,
            Amount = request.Amount,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.PayrollVariables.Add(variable);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new PayrollVariableSummary(variable.Id, variable.EmployeeId, variable.PeriodYear, variable.PeriodMonth, variable.Type.ToString(), variable.Label, variable.Amount);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var variable = await dbContext.PayrollVariables.FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
        if (variable is null)
        {
            return false;
        }

        dbContext.PayrollVariables.Remove(variable);
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}
