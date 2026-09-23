namespace Sirh.Api.Controllers.Dtos;

public sealed record LoginRequest(string Email, string Password, string? MfaCode);

public sealed record ConfirmMfaRequest(string Code);
