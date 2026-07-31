namespace HRestaurant.Configuration;

public sealed class InventoryAlertSettings
{
    public const string SectionName = "InventoryAlerts";
    public int CheckIntervalMinutes { get; set; } = 15;
    public int ExpiringSoonDays { get; set; } = 7;
}
