namespace HRestaurant.WebApi.Realtime;

public static class KitchenHubGroups
{
    public static string Restaurant(Guid id) => $"Restaurant:{id:N}";
    public static string Branch(Guid id) => $"Branch:{id:N}";
    public static string Kitchen(Guid id) => $"Kitchen:{id:N}";
    public static string Waiters(Guid id) => $"Waiters:{id:N}";
    public static string Waiter(Guid appUserId) => $"Waiter:{appUserId:N}";
}
