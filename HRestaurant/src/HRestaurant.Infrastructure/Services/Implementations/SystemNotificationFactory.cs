using HRestaurant.Enum;
using HRestaurant.Models;

namespace HRestaurant.Services.Implementations;

internal static class SystemNotificationFactory
{
    public static InventoryNotification ReservationCreated(
        Guid reservationId,
        Guid restaurantId,
        Guid branchId,
        string fullName,
        string confirmationCode,
        DateTime reservationTimeUtc,
        DateTime nowUtc) => new()
    {
        RestaurantId = restaurantId,
        BranchId = branchId,
        RelatedEntityId = reservationId,
        Type = InventoryAlertType.ReservationCreated,
        Title = "Yeni rezervasiya",
        Message = $"{fullName} üçün {reservationTimeUtc:dd.MM.yyyy HH:mm} tarixli rezervasiya yaradıldı. Kod: {confirmationCode}.",
        TargetUrl = $"/reservations?reservationId={reservationId}",
        CreatAt = nowUtc
    };

    public static InventoryNotification ReservationStatusChanged(
        Reservation reservation,
        Guid restaurantId,
        DateTime nowUtc) => new()
    {
        RestaurantId = restaurantId,
        BranchId = reservation.BranchId,
        RelatedEntityId = reservation.ID,
        Type = InventoryAlertType.ReservationStatusChanged,
        Title = "Rezervasiya statusu dəyişdi",
        Message = $"{reservation.ConfirmationCode} kodlu rezervasiya {reservation.Status} statusuna keçirildi.",
        TargetUrl = $"/reservations?reservationId={reservation.ID}",
        CreatAt = nowUtc
    };

    public static InventoryNotification OrderReady(Order order, DateTime nowUtc) => new()
    {
        RestaurantId = order.RestaurantId,
        BranchId = order.BranchId,
        RelatedEntityId = order.ID,
        Type = InventoryAlertType.OrderReady,
        Title = "Sifariş hazırdır",
        Message = order.Table is null
            ? $"{order.OrderNumber} sifarişi servis üçün hazırdır."
            : $"{order.OrderNumber} sifarişi (Masa {order.Table.TableNumber}) servis üçün hazırdır.",
        TargetUrl = $"/orders?search={Uri.EscapeDataString(order.OrderNumber)}",
        CreatAt = nowUtc
    };
}
