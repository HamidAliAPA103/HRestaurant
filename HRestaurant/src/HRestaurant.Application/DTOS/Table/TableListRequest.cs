using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;

namespace HRestaurant.DTOS.Table;

public sealed class TableListRequest
{
    public int PageNumber { get; set; } = PaginationRequest.DefaultPageNumber;
    public int PageSize { get; set; } = PaginationRequest.DefaultPageSize;
    public Guid? RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public string? Search { get; set; }
    public TableStatus? Status { get; set; }
    public bool? IsActive { get; set; }
}
