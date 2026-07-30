namespace HRestaurant.WebApi.RateLimiting;

public static class RateLimitPolicies
{
    public const string PublicGet = "public-get";
    public const string ReservationCreate = "reservation-create";
    public const string ReservationLookup = "reservation-lookup";
}
