namespace HRestaurant.DTOS.Auth;

public sealed record CurrentUserResponse(
    Guid UserId,
    string FullName,
    string Email,
    Guid RestaurantId,
    Guid? BranchId,
    IReadOnlyCollection<string> Roles,
    IReadOnlyCollection<string> Permissions,
    bool EmailConfirmed);
