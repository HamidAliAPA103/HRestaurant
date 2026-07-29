using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace HRestaurant.Infrastructure.Authorization;

public sealed class PermissionAuthorizationPolicyProvider
    : IAuthorizationPolicyProvider
{
    private const string PolicyPrefix = "Permission:";

    private readonly DefaultAuthorizationPolicyProvider _fallbackProvider;

    public PermissionAuthorizationPolicyProvider(
        IOptions<AuthorizationOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);
        _fallbackProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public bool AllowsCachingPolicies => true;

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(policyName);

        if (!policyName.StartsWith(
                PolicyPrefix,
                StringComparison.OrdinalIgnoreCase))
        {
            return _fallbackProvider.GetPolicyAsync(policyName);
        }

        var permission = policyName[PolicyPrefix.Length..];

        if (string.IsNullOrWhiteSpace(permission))
        {
            return Task.FromResult<AuthorizationPolicy?>(null);
        }

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(permission))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }

    public static string GetPolicyName(string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission);
        return $"{PolicyPrefix}{permission}";
    }
}
