using Microsoft.AspNetCore.Identity;

namespace Sirh.Infrastructure.Identity;

/// <summary>
/// Utilisateur de l'application : étend l'utilisateur Identity standard avec la société
/// (tenant) à laquelle il appartient et son nom complet.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>Société de l'utilisateur. Null pour un administrateur de la plateforme (aucune société).</summary>
    public Guid? TenantId { get; set; }

    public string FullName { get; set; } = string.Empty;
}
