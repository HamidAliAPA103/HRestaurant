namespace HRestaurant.Infrastructure.Authentication;

public static class AppRoles
{
    public const string SuperAdmin = nameof(SuperAdmin);

    public const string RestaurantOwner = nameof(RestaurantOwner);

    public const string Manager = nameof(Manager);

    public const string Cashier = nameof(Cashier);

    public const string Waiter = nameof(Waiter);

    public const string Chef = nameof(Chef);

    public static IReadOnlyCollection<string> All { get; } =
    [
        SuperAdmin,
        RestaurantOwner,
        Manager,
        Cashier,
        Waiter,
        Chef
    ];
}
