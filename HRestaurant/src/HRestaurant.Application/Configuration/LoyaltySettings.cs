namespace HRestaurant.Configuration;

public sealed class LoyaltySettings
{
    public const string SectionName = "Loyalty";
    public decimal EarnPointsPerCurrencyUnit { get; set; } = 1m;
    public decimal CurrencyValuePerPoint { get; set; } = 0.01m;
}
