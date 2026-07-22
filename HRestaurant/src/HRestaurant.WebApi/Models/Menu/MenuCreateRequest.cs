using Microsoft.AspNetCore.Http;

namespace HRestaurant.WebApi.Models.Menu;

public sealed class MenuCreateRequest
{
    public required IFormFile Image { get; init; }

    public decimal Price { get; init; }

    public required string Desc { get; init; }

    public Guid CategoryId { get; init; }

    public required string Nutrition { get; init; }
}
