using Microsoft.AspNetCore.Authorization;

namespace Sirh.Api.Security;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
