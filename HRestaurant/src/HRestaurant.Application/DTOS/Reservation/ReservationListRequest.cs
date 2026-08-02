using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.DTOS.Reservation;

public sealed class ReservationListRequest
{
    public int PageNumber { get; set; } = PaginationRequest.DefaultPageNumber;
    public int PageSize { get; set; } = PaginationRequest.DefaultPageSize;
    public Guid? BranchId { get; set; }
    public Guid? TableId { get; set; }
    public ReservationStatus? Status { get; set; }
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public string? Search { get; set; }
}

public sealed class ReservationStatusUpdateDTO
{
    public ReservationStatus Status { get; set; }
    public string? Reason { get; set; }
}
