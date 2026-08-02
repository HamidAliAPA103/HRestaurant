using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using HRestaurant.Configuration;
using HRestaurant.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRestaurant.Services.Implementations;

public sealed class SmtpAccountEmailSender : IAccountEmailSender
{
    private readonly ReservationEmailSettings _settings;
    private readonly ILogger<SmtpAccountEmailSender> _logger;

    public SmtpAccountEmailSender(
        ReservationEmailSettings settings,
        ILogger<SmtpAccountEmailSender> logger)
    {
        _settings = settings;
        _logger = logger;
    }

    public Task SendPasswordResetAsync(
        string email,
        string fullName,
        string resetUrl,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            email,
            fullName,
            "HRestaurant password reset",
            "Reset your password",
            "A password reset was requested for your account.",
            resetUrl,
            cancellationToken);

    public Task SendEmailVerificationAsync(
        string email,
        string fullName,
        string verificationUrl,
        CancellationToken cancellationToken = default) =>
        SendAsync(
            email,
            fullName,
            "Verify your HRestaurant email",
            "Verify your email",
            "Confirm this email address to complete account verification.",
            verificationUrl,
            cancellationToken);

    private async Task SendAsync(
        string email,
        string fullName,
        string subject,
        string heading,
        string message,
        string actionUrl,
        CancellationToken cancellationToken)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation(
                "Account email delivery is disabled. MessageType: {Subject}, Recipient: {Recipient}",
                subject,
                email);
            return;
        }

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(_settings.Username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(_settings.Username, _settings.Password)
        };
        using var mail = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject = subject,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true,
            Body = $"""
                <!doctype html><html><body style="font-family:Arial,sans-serif;color:#29231f">
                <h1>{HtmlEncoder.Default.Encode(heading)}</h1>
                <p>Hello {HtmlEncoder.Default.Encode(fullName)},</p>
                <p>{HtmlEncoder.Default.Encode(message)}</p>
                <p><a href="{HtmlEncoder.Default.Encode(actionUrl)}">Continue securely</a></p>
                <p>If you did not request this action, ignore this email.</p>
                </body></html>
                """
        };
        mail.To.Add(new MailAddress(email, fullName));
        try
        {
            await client.SendMailAsync(mail, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Account email could not be delivered. MessageType: {Subject}, Recipient: {Recipient}",
                subject,
                email);
        }
    }
}
