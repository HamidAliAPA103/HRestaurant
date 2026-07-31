using HRestaurant.DTOS.Responses;

namespace HRestaurant.DTOS.User;

public sealed class EmployeeListRequest
{
    public int PageNumber { get; set; } = PaginationRequest.DefaultPageNumber;
    public int PageSize { get; set; } = PaginationRequest.DefaultPageSize;
    public Guid? RestaurantId { get; set; }
    public Guid? BranchId { get; set; }
    public string? Search { get; set; }
    public string? Role { get; set; }
    public bool? IsActive { get; set; }
    public string SortBy { get; set; } = "name";
    public string SortDirection { get; set; } = "asc";
}
