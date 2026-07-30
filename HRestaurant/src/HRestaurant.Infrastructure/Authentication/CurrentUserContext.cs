using System.Security.Claims;
using HRestaurant.Exceptions;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Http;

namespace HRestaurant.Infrastructure.Authentication;

public sealed class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid UserId => GetGuidClaim(AuthClaimTypes.UserId);

    public Guid RestaurantId =>
        GetGuidClaim(AuthClaimTypes.RestaurantId);

    public bool IsSuperAdmin =>
        GetCurrentUser().IsInRole(AppRoles.SuperAdmin);

    private ClaimsPrincipal GetCurrentUser()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedException();
        }

        return user;
    }

    private Guid GetGuidClaim(string claimType)
    {
        var value = GetCurrentUser().FindFirstValue(claimType);

        if (!Guid.TryParse(value, out var id) || id == Guid.Empty)
        {
            throw new UnauthorizedException(
                $"The access token does not contain a valid "
                + $"'{claimType}' claim.");
        }

        return id;
    }
}
