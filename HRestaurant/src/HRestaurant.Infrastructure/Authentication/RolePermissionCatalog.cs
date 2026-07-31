namespace HRestaurant.Infrastructure.Authentication;

public static class RolePermissionCatalog
{
    private static readonly IReadOnlyDictionary<
        string,
        IReadOnlyCollection<string>> PermissionMap =
        new Dictionary<string, IReadOnlyCollection<string>>(
            StringComparer.OrdinalIgnoreCase)
        {
            [AppRoles.SuperAdmin] = [Permissions.All],
            [AppRoles.RestaurantOwner] =
            [
                Permissions.Restaurants.Read,
                Permissions.Restaurants.Manage,
                Permissions.Branches.Read,
                Permissions.Branches.Manage,
                Permissions.Employees.Read,
                Permissions.Employees.Manage,
                Permissions.Menus.Read,
                Permissions.Menus.Manage,
                Permissions.Shifts.Read,
                Permissions.Shifts.Manage,
                Permissions.Suppliers.Read,
                Permissions.Suppliers.Manage,
                Permissions.Inventory.Read,
                Permissions.Inventory.Manage,
                Permissions.Inventory.Adjust,
                Permissions.Notifications.Read,
                Permissions.Notifications.Manage,
                Permissions.Tables.Read,
                Permissions.Tables.Manage,
                Permissions.Reservations.Read,
                Permissions.Reservations.Manage,
                Permissions.Orders.Read,
                Permissions.Orders.Create,
                Permissions.Orders.Update,
                Permissions.Orders.Delete,
                Permissions.Orders.UpdateKitchenStatus,
                Permissions.Payments.Process,
                Permissions.Reviews.Read,
                Permissions.Reviews.Manage
            ],
            [AppRoles.Manager] = Permissions.AllValues,
            [AppRoles.Cashier] =
            [
                Permissions.Branches.Read,
                Permissions.Employees.Read,
                Permissions.Menus.Read,
                Permissions.Tables.Read,
                Permissions.Orders.Read,
                Permissions.Orders.Create,
                Permissions.Payments.Process
            ],
            [AppRoles.Waiter] =
            [
                Permissions.Branches.Read,
                Permissions.Employees.Read,
                Permissions.Menus.Read,
                Permissions.Tables.Read,
                Permissions.Reservations.Read,
                Permissions.Reservations.Manage,
                Permissions.Orders.Read,
                Permissions.Orders.Create,
                Permissions.Orders.Update
            ],
            [AppRoles.Chef] =
            [
                Permissions.Menus.Read,
                Permissions.Inventory.Read,
                Permissions.Notifications.Read,
                Permissions.Orders.Read,
                Permissions.Orders.UpdateKitchenStatus
            ]
        };

    public static IReadOnlyCollection<string> GetPermissions(string role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(role);

        return PermissionMap.TryGetValue(role, out var permissions)
            ? permissions
            : Array.Empty<string>();
    }
}
