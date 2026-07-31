using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.DTOS.Order;

public sealed class OrderListRequest
{
    public int PageNumber { get; set; } = PaginationRequest.DefaultPageNumber;
    public int PageSize { get; set; } = PaginationRequest.DefaultPageSize;
    public Guid? RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public Guid? WaiterId { get; set; }
    public OrderType? OrderType { get; set; }
    public OrderStatus? Status { get; set; }
    public string? Search { get; set; }
}
