namespace HRestaurant.Services.Interfaces;

public interface IReservationEmailSender
{
    Task SendAsync(
        ReservationEmailMessage message,
        CancellationToken cancellationToken = default);
}
