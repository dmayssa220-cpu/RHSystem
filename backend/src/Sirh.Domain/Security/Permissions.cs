namespace Sirh.Domain.Security;


public static class Permissions
{
    public const string GererUtilisateurs = "administration.utilisateurs.gerer";
    public const string GererRoles = "administration.roles.gerer";

    public const string LirePersonnel = "personnel.dossier.lire";
    public const string GererPersonnel = "personnel.dossier.gerer";

    public const string CalculerPaie = "paie.calcul.executer";

    /// <summary>Liste complète, utilisée pour donner toutes les permissions au rôle Administrateur au démarrage.</summary>
    public static readonly IReadOnlyList<string> Toutes = new[]
    {
        GererUtilisateurs,
        GererRoles,
        LirePersonnel,
        GererPersonnel,
        CalculerPaie
    };
}
