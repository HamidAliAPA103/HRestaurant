namespace HRestaurant.Services.Interfaces;

public interface ICurrentUserContext
{
    Guid UserId { get; }

    Guid RestaurantId { get; }

    bool IsSuperAdmin { get; }
}
