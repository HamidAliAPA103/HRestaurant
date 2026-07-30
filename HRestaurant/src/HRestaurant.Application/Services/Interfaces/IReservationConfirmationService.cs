namespace HRestaurant.Services.Interfaces;

public interface IReservationConfirmationService
{
    string GenerateConfirmationCode();

    string GenerateTrackingToken();

    string HashTrackingToken(string trackingToken);
}
