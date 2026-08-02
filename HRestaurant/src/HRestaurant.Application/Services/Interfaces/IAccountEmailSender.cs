namespace HRestaurant.Services.Interfaces;

public interface IAccountEmailSender
{
    Task SendPasswordResetAsync(
        string email,
        string fullName,
        string resetUrl,
        CancellationToken cancellationToken = default);

    Task SendEmailVerificationAsync(
        string email,
        string fullName,
        string verificationUrl,
        CancellationToken cancellationToken = default);
}
