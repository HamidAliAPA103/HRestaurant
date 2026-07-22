using Microsoft.AspNetCore.Http;

namespace HRestaurant.WebApi.Models.Menu;

public sealed class MenuUpdateRequest
{
    public IFormFile? Image { get; init; }

    public string? ImageURL { get; init; }

    public decimal? Price { get; init; }

    public string? Desc { get; init; }

    public string? Nutrition { get; init; }
}
