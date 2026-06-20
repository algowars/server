using Microsoft.AspNetCore.Authorization;

namespace Algowars.Api.Authorization;

public sealed class RbacRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } =
        permission ?? throw new ArgumentNullException(nameof(permission));
}