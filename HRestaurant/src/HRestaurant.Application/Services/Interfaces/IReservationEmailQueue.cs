namespace HRestaurant.Services.Interfaces;

public interface IReservationEmailQueue
{
    ValueTask QueueAsync(
        ReservationEmailMessage message,
        CancellationToken cancellationToken = default);
}

public sealed record ReservationEmailMessage(
    string RecipientEmail,
    string RecipientName,
    string ConfirmationCode,
    string RestaurantName,
    string BranchName,
    string BranchAddress,
    DateOnly ReservationDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    int GuestCount,
    string TableNumber,
    string TrackingUrl,
    string CancellationUrl);
