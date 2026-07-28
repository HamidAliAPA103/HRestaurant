using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HRestaurant.Services.Implementations;

public sealed class TokenService : ITokenService
{
    private const int RefreshTokenSizeInBytes = 64;

    private readonly JwtSettings _settings;
    private readonly TimeProvider _timeProvider;
    private readonly SigningCredentials _signingCredentials;

    public TokenService(
        IOptions<JwtSettings> settings,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _settings = settings.Value;
        _timeProvider = timeProvider;

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_settings.Key));

        _signingCredentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);
    }

    public AccessTokenResult CreateAccessToken(
        TokenUser user,
        IReadOnlyCollection<string> roles)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roles);

        var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
        var expiresAtUtc = nowUtc.AddMinutes(
            _settings.AccessTokenMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(AuthClaimTypes.UserId, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(AuthClaimTypes.RestaurantId, user.RestaurantId.ToString())
        };

        claims.AddRange(
            roles
                .Where(role => !string.IsNullOrWhiteSpace(role))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(role => new Claim(AuthClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: expiresAtUtc,
            signingCredentials: _signingCredentials);

        return new AccessTokenResult(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc);
    }

    public RefreshTokenResult CreateRefreshToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(
            RefreshTokenSizeInBytes);
        var token = Base64UrlEncoder.Encode(tokenBytes);
        var createdAtUtc = _timeProvider.GetUtcNow().UtcDateTime;

        return new RefreshTokenResult(
            token,
            HashRefreshToken(token),
            createdAtUtc,
            createdAtUtc.AddDays(_settings.RefreshTokenDays));
    }

    public string HashRefreshToken(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(refreshToken));

        return Base64UrlEncoder.Encode(hash);
    }
}
