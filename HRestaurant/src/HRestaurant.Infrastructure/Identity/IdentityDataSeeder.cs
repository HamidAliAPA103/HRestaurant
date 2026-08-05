using System.Security.Claims;
using HRestaurant.Infrastructure.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace HRestaurant.Infrastructure.Identity;

public static class IdentityDataSeeder
{
    public static async Task SeedIdentityDataAsync(
        this IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        await using var scope = serviceProvider.CreateAsyncScope();
        var roleManager =
            scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
        var timeProvider =
            scope.ServiceProvider.GetRequiredService<TimeProvider>();

        foreach (var roleName in AppRoles.All)
        {
                var role = await roleManager.FindByNameAsync(roleName);

            if (role is null)
            {
                role = new AppRole
                {
                    Id = Guid.NewGuid(),
                    Name = roleName,
                    CreatedAtUtc =
                        timeProvider.GetUtcNow().UtcDateTime
                };

                EnsureSucceeded(
                    await roleManager.CreateAsync(role),
                    $"Role '{roleName}' could not be seeded.");
            }

            var existingClaims =
                await roleManager.GetClaimsAsync(role);
            var expectedPermissions = new HashSet<string>(
                RolePermissionCatalog.GetPermissions(roleName),
                StringComparer.OrdinalIgnoreCase);

            foreach (var claim in existingClaims.Where(claim =>
                         IsManagedPermission(claim)
                         && !expectedPermissions.Contains(claim.Value)))
            {
                EnsureSucceeded(
                    await roleManager.RemoveClaimAsync(role, claim),
                    $"Permission '{claim.Value}' could not be removed "
                    + $"from role '{roleName}'.");
            }

            foreach (var permission in expectedPermissions)
            {
                var alreadyExists = existingClaims.Any(claim =>
                    string.Equals(
                        claim.Type,
                        AuthClaimTypes.Permission,
                        StringComparison.Ordinal)
                    && string.Equals(
                        claim.Value,
                        permission,
                        StringComparison.OrdinalIgnoreCase));

                if (alreadyExists)
                {
                    continue;
                }

                EnsureSucceeded(
                    await roleManager.AddClaimAsync(
                        role,
                        new Claim(
                            AuthClaimTypes.Permission,
                            permission)),
                    $"Permission '{permission}' could not be seeded "
                    + $"for role '{roleName}'.");
            }
        }
    }

    private static bool IsManagedPermission(Claim claim)
    {
        return string.Equals(
                   claim.Type,
                   AuthClaimTypes.Permission,
                   StringComparison.Ordinal)
               && (string.Equals(
                       claim.Value,
                       Permissions.All,
                       StringComparison.OrdinalIgnoreCase)
                   || Permissions.AllValues.Contains(
                       claim.Value,
                       StringComparer.OrdinalIgnoreCase));
    }

    private static void EnsureSucceeded(
        IdentityResult result,
        string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var errors = string.Join(
            "; ",
            result.Errors.Select(error =>
                $"{error.Code}: {error.Description}"));

        throw new InvalidOperationException($"{message} {errors}");
    }
}
