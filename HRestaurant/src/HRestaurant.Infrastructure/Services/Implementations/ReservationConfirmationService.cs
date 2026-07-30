using System.Security.Cryptography;
using System.Text;
using HRestaurant.Services.Interfaces;

namespace HRestaurant.Services.Implementations;

public sealed class ReservationConfirmationService
    : IReservationConfirmationService
{
    private const string ConfirmationAlphabet =
        "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

    public string GenerateConfirmationCode()
    {
        Span<char> characters = stackalloc char[6];

        for (var index = 0; index < characters.Length; index++)
        {
            characters[index] = ConfirmationAlphabet[
                RandomNumberGenerator.GetInt32(
                    ConfirmationAlphabet.Length)];
        }

        return $"RSV-{new string(characters)}";
    }

    public string GenerateTrackingToken()
    {
        return Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32))
            .ToLowerInvariant();
    }

    public string HashTrackingToken(string trackingToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingToken);

        return Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(trackingToken.Trim())))
            .ToLowerInvariant();
    }
}
