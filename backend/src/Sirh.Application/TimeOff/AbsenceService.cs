using Microsoft.EntityFrameworkCore;
using Sirh.Application.Persistence;
using Sirh.Application.Security;
using Sirh.Domain.TimeOff;

namespace Sirh.Application.TimeOff;

/// <summary>
/// Demandes d'absence et leur validation. Seule une absence "SansSolde" validée affecte la
/// paie (voir Sirh.Application.Payroll.PayrollService) — en attente ou refusée, elle n'a aucun effet.
/// </summary>
public sealed class AbsenceService(IAppDbContext dbContext, ICurrentUserService currentUser)
{
    public async Task<IReadOnlyList<AbsenceSummary>> ListAsync(CancellationToken cancellationToken = default) =>
        await (from a in dbContext.Absences
               join e in dbContext.Employees on a.EmployeeId equals e.Id
               orderby a.StartDate descending
               select new AbsenceSummary(a.Id, a.EmployeeId, e.FirstName, e.LastName, a.Type.ToString(), a.StartDate, a.EndDate, a.Status.ToString(), a.Comment))
              .ToListAsync(cancellationToken);

    public async Task<AbsenceSummary> RequestAsync(RequestAbsenceRequest request, CancellationToken cancellationToken = default)
    {
        if (currentUser.TenantId is not { } tenantId)
        {
            throw new InvalidOperationException("Aucune société associée à l'utilisateur courant.");
        }

        if (request.EndDate < request.StartDate)
        {
            throw new InvalidOperationException("La date de fin ne peut pas précéder la date de début.");
        }

        var employee = await dbContext.Employees.FirstOrDefaultAsync(e => e.Id == request.EmployeeId, cancellationToken)
            ?? throw new InvalidOperationException("Salarié introuvable.");

        var absence = new Absence
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            EmployeeId = request.EmployeeId,
            Type = Enum.Parse<AbsenceType>(request.Type, ignoreCase: true),
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Status = AbsenceStatus.EnAttente,
            Comment = request.Comment,
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Absences.Add(absence);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new AbsenceSummary(absence.Id, absence.EmployeeId, employee.FirstName, employee.LastName, absence.Type.ToString(), absence.StartDate, absence.EndDate, absence.Status.ToString(), absence.Comment);
    }

    public async Task<AbsenceSummary?> DecideAsync(Guid id, DecideAbsenceRequest request, CancellationToken cancellationToken = default)
    {
        var absence = await dbContext.Absences.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (absence is null)
        {
            return null;
        }

        if (absence.Status != AbsenceStatus.EnAttente)
        {
            throw new InvalidOperationException("Cette demande a déjà été traitée.");
        }

        absence.Status = request.Approve ? AbsenceStatus.Validee : AbsenceStatus.Refusee;
        absence.Comment = request.Comment ?? absence.Comment;
        absence.DecidedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        var employee = await dbContext.Employees.FirstAsync(e => e.Id == absence.EmployeeId, cancellationToken);
        return new AbsenceSummary(absence.Id, absence.EmployeeId, employee.FirstName, employee.LastName, absence.Type.ToString(), absence.StartDate, absence.EndDate, absence.Status.ToString(), absence.Comment);
    }
}
