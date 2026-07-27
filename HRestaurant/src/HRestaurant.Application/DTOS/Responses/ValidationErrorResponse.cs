namespace HRestaurant.DTOS.Responses;

public sealed class ValidationErrorResponse
{
    public int StatusCode { get; init; } = 400;

    public string Message { get; init; } =
        "One or more validation errors occurred.";

    public IReadOnlyCollection<ValidationErrorDTO> Errors { get; init; } =
        Array.Empty<ValidationErrorDTO>();
}
