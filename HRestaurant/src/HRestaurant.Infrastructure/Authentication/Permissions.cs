namespace HRestaurant.Infrastructure.Authentication;

public static class Permissions
{
    public const string All = "*";

    public static class Restaurants
    {
        public const string Read = "restaurants.read";
        public const string Manage = "restaurants.manage";
    }

    public static class Employees
    {
        public const string Read = "employees.read";
        public const string Manage = "employees.manage";
    }

    public static class Menus
    {
        public const string Read = "menus.read";
        public const string Manage = "menus.manage";
    }

    public static class Tables
    {
        public const string Read = "tables.read";
        public const string Manage = "tables.manage";
    }

    public static class Reservations
    {
        public const string Read = "reservations.read";
        public const string Manage = "reservations.manage";
    }

    public static class Orders
    {
        public const string Read = "orders.read";
        public const string Create = "orders.create";
        public const string Update = "orders.update";
        public const string Delete = "orders.delete";
        public const string UpdateKitchenStatus =
            "orders.kitchen-status.update";
    }

    public static class Payments
    {
        public const string Process = "payments.process";
    }

    public static class Reviews
    {
        public const string Read = "reviews.read";
        public const string Manage = "reviews.manage";
    }

    public static IReadOnlyCollection<string> AllValues { get; } =
    [
        Restaurants.Read,
        Restaurants.Manage,
        Employees.Read,
        Employees.Manage,
        Menus.Read,
        Menus.Manage,
        Tables.Read,
        Tables.Manage,
        Reservations.Read,
        Reservations.Manage,
        Orders.Read,
        Orders.Create,
        Orders.Update,
        Orders.Delete,
        Orders.UpdateKitchenStatus,
        Payments.Process,
        Reviews.Read,
        Reviews.Manage
    ];
}
