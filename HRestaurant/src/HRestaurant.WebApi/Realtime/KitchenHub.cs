using System.Security.Claims;
using HRestaurant.Data;
using HRestaurant.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.WebApi.Realtime;

[Authorize]
public sealed class KitchenHub : Hub
{
    private readonly AppDbContext _db;
    public KitchenHub(AppDbContext db) => _db = db;

    public override async Task OnConnectedAsync()
    {
        var principal = Context.User;
        if (principal?.Identity?.IsAuthenticated != true
            || !Guid.TryParse(principal.FindFirstValue(AuthClaimTypes.UserId), out var userId))
        {
            Context.Abort();
            return;
        }

        var restaurantId = Guid.TryParse(
            principal.FindFirstValue(AuthClaimTypes.RestaurantId), out var parsedRestaurantId)
            ? parsedRestaurantId : (Guid?)null;
        if (restaurantId.HasValue)
            await Groups.AddToGroupAsync(Context.ConnectionId,
                KitchenHubGroups.Restaurant(restaurantId.Value));

        var branchIds = await ResolveBranchIdsAsync(userId, restaurantId, principal);
        foreach (var branchId in branchIds)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, KitchenHubGroups.Branch(branchId));
            if (principal.IsInRole(AppRoles.Chef) || principal.IsInRole(AppRoles.Manager)
                || principal.IsInRole(AppRoles.RestaurantOwner)
                || principal.IsInRole(AppRoles.SuperAdmin))
                await Groups.AddToGroupAsync(Context.ConnectionId, KitchenHubGroups.Kitchen(branchId));
            if (principal.IsInRole(AppRoles.Waiter))
                await Groups.AddToGroupAsync(Context.ConnectionId, KitchenHubGroups.Waiters(branchId));
        }

        if (principal.IsInRole(AppRoles.Waiter))
            await Groups.AddToGroupAsync(Context.ConnectionId, KitchenHubGroups.Waiter(userId));

        await base.OnConnectedAsync();
    }

    private async Task<IReadOnlyCollection<Guid>> ResolveBranchIdsAsync(
        Guid userId, Guid? restaurantId, ClaimsPrincipal principal)
    {
        if (principal.IsInRole(AppRoles.SuperAdmin))
            return await _db.Branches.AsNoTracking()
                .Where(x => x.IsActive && !x.IsDeleted)
                .Select(x => x.ID).ToArrayAsync(Context.ConnectionAborted);

        if (principal.IsInRole(AppRoles.RestaurantOwner) && restaurantId.HasValue)
            return await _db.Branches.AsNoTracking()
                .Where(x => x.RestaurantId == restaurantId && x.IsActive && !x.IsDeleted)
                .Select(x => x.ID).ToArrayAsync(Context.ConnectionAborted);

        if (principal.IsInRole(AppRoles.Manager))
            return await _db.Branches.AsNoTracking()
                .Where(x => x.ManagerId == userId && x.IsActive && !x.IsDeleted)
                .Select(x => x.ID).ToArrayAsync(Context.ConnectionAborted);

        var branchId = await _db.BusinessUsers.AsNoTracking()
            .Where(x => x.AppUserId == userId && x.IsActive && !x.IsDeleted)
            .Select(x => x.BranchId)
            .FirstOrDefaultAsync(Context.ConnectionAborted);
        return branchId.HasValue ? [branchId.Value] : [];
    }
}
