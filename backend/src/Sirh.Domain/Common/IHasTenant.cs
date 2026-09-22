namespace Sirh.Domain.Common;

/// <summary>
/// Marque une entité comme appartenant à une société (tenant).
/// Sert de point d'accroche au filtre global EF Core qui cloisonnera les données
/// entre sociétés (mis en place à l'étape « socle de sécurité »).
/// </summary>
public interface IHasTenant
{
    Guid TenantId { get; set; }
}
