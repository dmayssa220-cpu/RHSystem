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

    public const string LirePersonnel = "personnel.dossier.lire";
    public const string GererPersonnel = "personnel.dossier.gerer";

    public const string CalculerPaie = "paie.calcul.executer";
    public const string GererDeclarations = "declarations.gerer";
    public const string GererVariablesPaie = "paie.variables.gerer";

    public const string LireAbsences = "temps.absences.lire";
    public const string GererAbsences = "temps.absences.gerer";

    public const string LireAnomalies = "conformite.anomalies.lire";
    public const string GererAnomalies = "conformite.anomalies.gerer";

    /// <summary>Liste complète, utilisée pour donner toutes les permissions au rôle Administrateur au démarrage.</summary>
    public static readonly IReadOnlyList<string> Toutes = new[]
    {
        GererUtilisateurs,
        GererRoles,
        LirePersonnel,
        GererPersonnel,
        CalculerPaie,
        GererDeclarations,
        GererVariablesPaie,
        LireAbsences,
        GererAbsences,
        LireAnomalies,
        GererAnomalies
    };
}
