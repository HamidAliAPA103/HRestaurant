using HRestaurant.DTOS.Responses;

namespace HRestaurant.DTOS.Shift;

public sealed class ShiftListRequest
{
    public int PageNumber { get; set; } = PaginationRequest.DefaultPageNumber;
    public int PageSize { get; set; } = PaginationRequest.DefaultPageSize;
    public Guid? RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? EmployeeId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
}
