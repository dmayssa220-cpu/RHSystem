namespace Sirh.Domain.Payroll;

/// <summary>Une tranche du barème IRPP : taux appliqué jusqu'à la limite supérieure (null = pas de limite, dernière tranche).</summary>
public sealed record LegalBracket(decimal? UpperBoundAnnual, decimal Rate);
