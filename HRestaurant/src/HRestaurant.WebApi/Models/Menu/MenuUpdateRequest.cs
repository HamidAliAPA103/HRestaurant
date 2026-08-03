using Microsoft.AspNetCore.Http;
using HRestaurant.DTOS.Menu;

namespace HRestaurant.WebApi.Models.Menu;

public sealed class MenuUpdateRequest
{
    public IFormFile? Image { get; init; }

    public string? ImageURL { get; init; }

    public string? Model3DUrl { get; init; }

    public string? ModelPosterUrl { get; init; }

    public decimal? ModelScale { get; init; }

    public decimal? ModelRotationX { get; init; }

    public decimal? ModelRotationY { get; init; }

    public decimal? ModelRotationZ { get; init; }

    public bool? Is3DEnabled { get; init; }

    public string? Name { get; init; }

    public decimal? Price { get; init; }

    public decimal? DiscountPercentage { get; init; }

    public int? PreparationTimeMinutes { get; init; }

    public Guid? CategoryId { get; init; }

    public string? Desc { get; init; }

    public string? Nutrition { get; init; }

    public List<MenuItemIngredientDTO>? Ingredients { get; init; }
}
