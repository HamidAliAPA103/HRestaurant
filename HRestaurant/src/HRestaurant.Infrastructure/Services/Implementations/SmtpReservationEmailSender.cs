using System.Net;
using System.Net.Mail;
using System.Text;
using System.Text.Encodings.Web;
using HRestaurant.Configuration;
using HRestaurant.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace HRestaurant.Services.Implementations;

public sealed class SmtpReservationEmailSender
    : IReservationEmailSender
{
    private readonly ReservationEmailSettings _settings;
    private readonly ILogger<SmtpReservationEmailSender> _logger;

    public SmtpReservationEmailSender(
        ReservationEmailSettings settings,
        ILogger<SmtpReservationEmailSender> logger)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(logger);

        _settings = settings;
        _logger = logger;
    }

    public async Task SendAsync(
        ReservationEmailMessage message,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(message);

        if (!_settings.Enabled)
        {
            _logger.LogInformation(
                "Reservation email delivery is disabled. "
                + "ConfirmationCode: {ConfirmationCode}",
                message.ConfirmationCode);
            return;
        }

        using var client = new SmtpClient(
            _settings.Host,
            _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = string.IsNullOrWhiteSpace(_settings.Username)
                ? CredentialCache.DefaultNetworkCredentials
                : new NetworkCredential(
                    _settings.Username,
                    _settings.Password)
        };

        using var mail = new MailMessage
        {
            From = new MailAddress(
                _settings.FromAddress,
                _settings.FromName),
            Subject =
                $"Reservation {message.ConfirmationCode} confirmed",
            Body = BuildHtmlBody(message),
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };
        mail.To.Add(new MailAddress(
            message.RecipientEmail,
            message.RecipientName));

        await client.SendMailAsync(mail, cancellationToken);
    }

    private static string BuildHtmlBody(
        ReservationEmailMessage message)
    {
        static string Encode(string value) =>
            HtmlEncoder.Default.Encode(value);

        return $"""
            <!doctype html>
            <html lang="en">
            <body style="font-family:Arial,sans-serif;color:#29231f">
              <h1>Reservation confirmed</h1>
              <p>Hello {Encode(message.RecipientName)},</p>
              <p>Your confirmation code is
                 <strong>{Encode(message.ConfirmationCode)}</strong>.</p>
              <ul>
                <li>{Encode(message.RestaurantName)} —
                    {Encode(message.BranchName)}</li>
                <li>{Encode(message.BranchAddress)}</li>
                <li>{message.ReservationDate:yyyy-MM-dd},
                    {message.StartTime:HH\:mm}–{message.EndTime:HH\:mm}</li>
                <li>{message.GuestCount} guests,
                    table {Encode(message.TableNumber)}</li>
              </ul>
              <p><a href="{Encode(message.TrackingUrl)}">
                 View reservation</a></p>
              <p><a href="{Encode(message.CancellationUrl)}">
                 Cancel reservation</a></p>
            </body>
            </html>
            """;
    }
}
