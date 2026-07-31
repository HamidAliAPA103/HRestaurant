using Microsoft.AspNetCore.Http;
using HRestaurant.DTOS.Menu;

namespace HRestaurant.WebApi.Models.Menu;

public sealed class MenuCreateRequest
{
    public IFormFile? Image { get; init; }

    public string? ImageUrl { get; init; }

    public required string Name { get; init; }

    public decimal Price { get; init; }

    public decimal DiscountPercentage { get; init; }

    public int PreparationTimeMinutes { get; init; }

    public required string Desc { get; init; }

    public Guid CategoryId { get; init; }

    public required string Nutrition { get; init; }

    public List<MenuItemIngredientDTO> Ingredients { get; init; } = [];
}
