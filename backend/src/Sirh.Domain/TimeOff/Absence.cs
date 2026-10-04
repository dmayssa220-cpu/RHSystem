using Sirh.Domain.Common;

namespace Sirh.Domain.TimeOff;

/// <summary>
/// Demande d'absence d'un salarié (congé payé, maladie, sans solde…). Seules les absences
/// "SansSolde" au statut Validee affectent le calcul de paie (voir Sirh.Application.Payroll.PayrollService) :
/// une absence en attente ou refusée n'a aucun effet sur le salaire.
/// </summary>
public sealed class Absence : IHasTenant
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid EmployeeId { get; set; }

    public AbsenceType Type { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public AbsenceStatus Status { get; set; } = AbsenceStatus.EnAttente;

    public string? Comment { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? DecidedAtUtc { get; set; }
}
