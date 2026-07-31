using HRestaurant.DTOS.Responses;

namespace HRestaurant.DTOS.Branch;

public sealed class BranchListRequest
{
    public int PageNumber { get; set; } =
        PaginationRequest.DefaultPageNumber;

    public int PageSize { get; set; } =
        PaginationRequest.DefaultPageSize;

    public string? Search { get; set; }

    public bool? IsActive { get; set; }

    public Guid? ManagerId { get; set; }

    public Guid? RestaurantId { get; set; }
}
