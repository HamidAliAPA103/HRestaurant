namespace HRestaurant.DTOS.Responses;

public sealed record ValidationErrorDTO(
    string Field,
    string Message);
