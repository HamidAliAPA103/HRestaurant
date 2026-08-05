using Microsoft.AspNetCore.Http;
using HRestaurant.DTOS.Menu;

namespace HRestaurant.WebApi.Models.Menu;

public sealed class MenuCreateRequest
{
    public IFormFile? Image { get; init; }

    public string? ImageUrl { get; init; }

    public string? Model3DUrl { get; init; }

    public string? ModelPosterUrl { get; init; }

    public decimal ModelScale { get; init; } = 1m;

    public decimal ModelRotationX { get; init; }

    public decimal ModelRotationY { get; init; }

    public decimal ModelRotationZ { get; init; }

    public bool Is3DEnabled { get; init; }
    public bool EnableIngredientAnimation { get; init; }
    public string? VideoUrl { get; init; }
    public string? VideoPosterUrl { get; init; }
    public int? VideoDurationSeconds { get; init; }
    public bool IsVideoEnabled { get; init; }
    public int VideoDisplayOrder { get; init; }

    public required string Name { get; init; }

    public decimal Price { get; init; }

    public decimal DiscountPercentage { get; init; }

    public int PreparationTimeMinutes { get; init; }

    public required string Desc { get; init; }

    public Guid CategoryId { get; init; }

    public required string Nutrition { get; init; }

    public List<MenuItemIngredientDTO> Ingredients { get; init; } = [];
}
