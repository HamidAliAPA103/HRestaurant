namespace HRestaurant.Services.Interfaces;

public interface IPublicRequestChallengeValidator
{
    Task EnsureValidAsync(
        string? challengeToken,
        string action,
        CancellationToken cancellationToken = default);
}
