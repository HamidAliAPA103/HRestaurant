using HRestaurant.DTOS.Responses;

namespace HRestaurant.DTOS.Ingredient;

public sealed class IngredientListRequest
{
    public int PageNumber { get; set; } = PaginationRequest.DefaultPageNumber;
    public int PageSize { get; set; } = PaginationRequest.DefaultPageSize;
    public Guid? RestaurantId { get; set; }
    public string? Search { get; set; }
    public bool? IsActive { get; set; }
}
