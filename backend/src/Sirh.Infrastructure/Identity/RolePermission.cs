namespace Sirh.Infrastructure.Identity;

/// <summary>
/// Association entre un rôle et une permission (clé composite RoleId + Permission).
/// Un rôle donné une ou plusieurs permissions : c'est ce que le code vérifie, jamais le nom du rôle.
/// </summary>
public sealed class RolePermission
{
    public Guid RoleId { get; set; }
    public string Permission { get; set; } = string.Empty;
}
