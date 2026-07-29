using Microsoft.AspNetCore.Authorization;

namespace HRestaurant.Infrastructure.Authorization;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = true,
    Inherited = true)]
public sealed class PermissionAuthorizeAttribute : AuthorizeAttribute
{
    public PermissionAuthorizeAttribute(string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);

        Permission = permission;
        Policy =
            PermissionAuthorizationPolicyProvider.GetPolicyName(permission);
    }

    public string Permission { get; }
}
