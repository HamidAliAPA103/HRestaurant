namespace HRestaurant.Configuration;

public sealed class ReservationEmailSettings
{
    public const string SectionName = "ReservationEmail";

    public bool Enabled { get; set; }

    public string Host { get; set; } = string.Empty;

    public int Port { get; set; } = 587;

    public bool EnableSsl { get; set; } = true;

    public string Username { get; set; } = string.Empty;

    public string Password { get; set; } = string.Empty;

    public string FromAddress { get; set; } =
        "reservations@example.com";

    public string FromName { get; set; } = "HRestaurant";
}
