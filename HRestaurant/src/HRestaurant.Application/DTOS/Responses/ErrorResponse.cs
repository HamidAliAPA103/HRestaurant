namespace HRestaurant.DTOS.Responses;

public sealed record ErrorResponse(
    string Code,
    string Message,
    string? Field = null);
