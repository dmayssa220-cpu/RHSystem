namespace Sirh.Infrastructure.Identity;

/// <summary>
/// Jeton de rafraîchissement. Seul le hachage (SHA-256) est stocké, jamais la valeur en clair.
/// Rotation à chaque utilisation : le jeton utilisé est révoqué et un nouveau est émis.
/// </summary>
public sealed class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? RevokedAtUtc { get; set; }
}
