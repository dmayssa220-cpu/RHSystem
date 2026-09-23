namespace Sirh.Application.Security;

/// <summary>Création des jetons d'accès (JWT) et de rafraîchissement. Implémentée dans Sirh.Api.</summary>
public interface ITokenService
{
    (string AccessToken, DateTime ExpiresAtUtc) CreateAccessToken(TokenUserInfo user);

    (string RawToken, string TokenHash, DateTime ExpiresAtUtc) CreateRefreshToken();

    /// <summary>Hache un jeton de rafraîchissement pour comparaison avec la valeur stockée en base (jamais en clair).</summary>
    string Hash(string rawToken);
}

public sealed record TokenUserInfo(
    Guid UserId,
    Guid? TenantId,
    string Email,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions);
