using HRestaurant.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;

namespace HRestaurant.Infrastructure.Authorization;

public sealed class PermissionHandler
    : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        if (context.User.IsInRole(AppRoles.SuperAdmin)
            || HasPermission(context, Permissions.All)
            || HasPermission(context, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }

    private static bool HasPermission(
        AuthorizationHandlerContext context,
        string permission)
    {
        return context.User.Claims.Any(claim =>
            string.Equals(
                claim.Type,
                AuthClaimTypes.Permission,
                StringComparison.Ordinal)
            && string.Equals(
                claim.Value,
                permission,
                StringComparison.OrdinalIgnoreCase));
    }
}
