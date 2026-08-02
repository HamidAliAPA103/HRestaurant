namespace HRestaurant.Services.Interfaces;

public interface ITokenService
{
    AccessTokenResult CreateAccessToken(
        TokenUser user,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions);

    RefreshTokenResult CreateRefreshToken();

    string HashRefreshToken(string refreshToken);
}

public sealed record TokenUser(
    Guid Id,
    string Email,
    Guid RestaurantId,
    string FullName,
    Guid? BranchId);

public sealed record AccessTokenResult(
    string Token,
    DateTime ExpiresAtUtc);

public sealed record RefreshTokenResult(
    string Token,
    string TokenHash,
    DateTime CreatedAtUtc,
    DateTime ExpiresAtUtc);
