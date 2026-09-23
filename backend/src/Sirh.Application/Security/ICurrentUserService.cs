namespace Sirh.Application.Security;

/// <summary>
/// Donne accès, depuis n'importe quelle couche, à l'utilisateur de la requête HTTP en cours
/// (implémentée dans Sirh.Api, qui seul connaît le contexte HTTP). Utilisée notamment par
/// le journal d'audit pour savoir qui a fait quoi.
/// </summary>
public interface ICurrentUserService
{
    Guid? UserId { get; }
    Guid? TenantId { get; }
    IReadOnlyCollection<string> Permissions { get; }
}
