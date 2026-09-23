using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Sirh.Api.Controllers.Dtos;
using Sirh.Application.Security;
using Sirh.Infrastructure;
using Sirh.Infrastructure.Identity;

namespace Sirh.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("auth")]
public sealed class AuthController(
    UserManager<ApplicationUser> userManager,
    SignInManager<ApplicationUser> signInManager,
    SirhDbContext dbContext,
    ITokenService tokenService,
    IHostEnvironment environment) : ControllerBase
{
    [HttpPost("connexion")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var user = await userManager.FindByEmailAsync(request.Email);
        if (user is null)
        {
            return Unauthorized(new { message = "Identifiants invalides." });
        }

        var passwordCheck = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!passwordCheck.Succeeded)
        {
            return Unauthorized(new { message = "Identifiants invalides." });
        }

        if (await userManager.GetTwoFactorEnabledAsync(user))
        {
            if (string.IsNullOrWhiteSpace(request.MfaCode))
            {
                return Ok(new { requiresMfa = true });
            }

            var mfaValid = await userManager.VerifyTwoFactorTokenAsync(
                user, TokenOptions.DefaultAuthenticatorProvider, request.MfaCode);

            if (!mfaValid)
            {
                return Unauthorized(new { message = "Code d'authentification invalide." });
            }
        }

        return await IssueTokensAsync(user);
    }

    [HttpPost("rafraichir")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh()
    {
        if (!Request.Cookies.TryGetValue("refresh_token", out var rawToken) || string.IsNullOrWhiteSpace(rawToken))
        {
            return Unauthorized();
        }

        var tokenHash = tokenService.Hash(rawToken);
        var stored = await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);

        if (stored is null || stored.RevokedAtUtc is not null || stored.ExpiresAtUtc < DateTime.UtcNow)
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(stored.UserId.ToString());
        if (user is null)
        {
            return Unauthorized();
        }

        stored.RevokedAtUtc = DateTime.UtcNow;
        return await IssueTokensAsync(user);
    }

    [HttpPost("deconnexion")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        if (Request.Cookies.TryGetValue("refresh_token", out var rawToken) && !string.IsNullOrWhiteSpace(rawToken))
        {
            var tokenHash = tokenService.Hash(rawToken);
            var stored = await dbContext.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
            if (stored is not null)
            {
                stored.RevokedAtUtc = DateTime.UtcNow;
                await dbContext.SaveChangesAsync();
            }
        }

        Response.Cookies.Delete("refresh_token", new CookieOptions { Path = "/api/auth" });
        return NoContent();
    }

    [HttpPost("mfa/activer")]
    [Authorize]
    public async Task<IActionResult> EnableMfa()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        await userManager.ResetAuthenticatorKeyAsync(user);
        var key = await userManager.GetAuthenticatorKeyAsync(user);
        var otpauthUri = $"otpauth://totp/SIRH:{user.Email}?secret={key}&issuer=SIRH&digits=6";

        return Ok(new { sharedKey = key, otpauthUri });
    }

    [HttpPost("mfa/confirmer")]
    [Authorize]
    public async Task<IActionResult> ConfirmMfa(ConfirmMfaRequest request)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var isValid = await userManager.VerifyTwoFactorTokenAsync(user, TokenOptions.DefaultAuthenticatorProvider, request.Code);
        if (!isValid)
        {
            return BadRequest(new { message = "Code invalide." });
        }

        await userManager.SetTwoFactorEnabledAsync(user, true);
        return NoContent();
    }

    private async Task<IActionResult> IssueTokensAsync(ApplicationUser user)
    {
        var roleNames = (await userManager.GetRolesAsync(user)).ToArray();

        var roleIds = await dbContext.Roles
            .Where(r => roleNames.Contains(r.Name!))
            .Select(r => r.Id)
            .ToListAsync();

        var permissions = (await dbContext.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission)
            .Distinct()
            .ToListAsync())
            .ToArray();

        var (accessToken, expiresAtUtc) = tokenService.CreateAccessToken(
            new TokenUserInfo(user.Id, user.TenantId, user.Email!, roleNames, permissions));

        var refresh = tokenService.CreateRefreshToken();

        dbContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refresh.TokenHash,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = refresh.ExpiresAtUtc
        });
        await dbContext.SaveChangesAsync();

        Response.Cookies.Append("refresh_token", refresh.RawToken, new CookieOptions
        {
            HttpOnly = true,
            Secure = !environment.IsDevelopment(),
            SameSite = SameSiteMode.Strict,
            Expires = refresh.ExpiresAtUtc,
            Path = "/api/auth"
        });

        return Ok(new
        {
            accessToken,
            expiresAtUtc,
            user = new { user.Id, user.Email, roles = roleNames }
        });
    }
}
