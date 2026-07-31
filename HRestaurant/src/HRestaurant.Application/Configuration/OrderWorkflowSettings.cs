namespace HRestaurant.Configuration;

public sealed class OrderWorkflowSettings
{
    public const string SectionName = "OrderWorkflow";

    public int DelayedAfterMinutes { get; set; } = 20;
}
