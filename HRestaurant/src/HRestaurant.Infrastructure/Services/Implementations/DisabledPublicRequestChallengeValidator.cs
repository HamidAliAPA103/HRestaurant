using HRestaurant.Services.Interfaces;

namespace HRestaurant.Services.Implementations;

public sealed class DisabledPublicRequestChallengeValidator
    : IPublicRequestChallengeValidator
{
    public Task EnsureValidAsync(
        string? challengeToken,
        string action,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
