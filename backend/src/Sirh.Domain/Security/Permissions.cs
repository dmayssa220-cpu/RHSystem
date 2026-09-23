namespace Sirh.Domain.Security;

/// <summary>
/// Permissions applicatives. Le code autorise des permissions (ex. [Authorize(Policy =
/// "permission:administration.utilisateurs.gerer")]), jamais des noms de rôle : un rôle
/// n'est qu'un regroupement de permissions, modifiable par société sans toucher au code.
/// </summary>
public static class Permissions
{
    public const string GererUtilisateurs = "administration.utilisateurs.gerer";
    public const string GererRoles = "administration.roles.gerer";

    /// <summary>Liste complète, utilisée pour donner toutes les permissions au rôle Administrateur au démarrage.</summary>
    public static readonly IReadOnlyList<string> Toutes = new[]
    {
        GererUtilisateurs,
        GererRoles
    };
}
