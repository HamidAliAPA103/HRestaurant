using System.Globalization;
using System.Threading.RateLimiting;
using HRestaurant.DTOS.Responses;
using Microsoft.AspNetCore.RateLimiting;

namespace HRestaurant.WebApi.RateLimiting;

public static class PublicRateLimitingExtensions
{
    public static IServiceCollection AddPublicRateLimiting(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode =
                StatusCodes.Status429TooManyRequests;
            options.OnRejected = WriteRateLimitResponseAsync;

            options.AddPolicy(
                RateLimitPolicies.PublicGet,
                context => CreatePartition(
                    context,
                    RateLimitPolicies.PublicGet,
                    permitLimit: 60,
                    window: TimeSpan.FromMinutes(1)));

            options.AddPolicy(
                RateLimitPolicies.ReservationCreate,
                context => CreatePartition(
                    context,
                    RateLimitPolicies.ReservationCreate,
                    permitLimit: 5,
                    window: TimeSpan.FromMinutes(10)));

            options.AddPolicy(
                RateLimitPolicies.ReservationLookup,
                context => CreatePartition(
                    context,
                    RateLimitPolicies.ReservationLookup,
                    permitLimit: 10,
                    window: TimeSpan.FromMinutes(10)));
        });

        return services;
    }

    private static RateLimitPartition<string> CreatePartition(
        HttpContext context,
        string policyName,
        int permitLimit,
        TimeSpan window)
    {
        var address =
            context.Connection.RemoteIpAddress?.ToString()
            ?? "unknown";
        var key = $"{policyName}:{address}";

        return RateLimitPartition.GetFixedWindowLimiter(
            key,
            _ => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = permitLimit,
                QueueLimit = 0,
                Window = window
            });
    }

    private static async ValueTask WriteRateLimitResponseAsync(
        OnRejectedContext context,
        CancellationToken cancellationToken)
    {
        var httpContext = context.HttpContext;

        if (context.Lease.TryGetMetadata(
                MetadataName.RetryAfter,
                out var retryAfter))
        {
            httpContext.Response.Headers.RetryAfter =
                Math.Ceiling(retryAfter.TotalSeconds)
                    .ToString(CultureInfo.InvariantCulture);
        }

        var response = ApiResponse.Failure<object?>(
            StatusCodes.Status429TooManyRequests,
            "Too many requests. Please try again later.",
            [
                new ErrorResponse(
                    "rate_limit_exceeded",
                    "The request limit for this endpoint was exceeded.")
            ],
            httpContext.TraceIdentifier);

        httpContext.Response.StatusCode =
            StatusCodes.Status429TooManyRequests;
        httpContext.Response.Headers.CacheControl = "no-store";

        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken);
    }
}
