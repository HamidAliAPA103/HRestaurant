using FluentValidation.Results;
using HRestaurant.DTOS.Responses;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HRestaurant.WebApi.Validation;

internal static class ValidationErrorResponseFactory
{
    private const string ValidationMessage =
        "One or more validation errors occurred.";

    public static ApiResponse<object?> FromFailures(
        IEnumerable<ValidationFailure> failures)
    {
        ArgumentNullException.ThrowIfNull(failures);

        var errors = failures
            .Where(failure => failure is not null)
            .Select(failure => new ErrorResponse(
                "validation_error",
                failure.ErrorMessage,
                failure.PropertyName))
            .Distinct()
            .ToArray();

        return Create(errors);
    }

    public static ApiResponse<object?> FromModelState(
        ModelStateDictionary modelState)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        var errors = modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error =>
                new ErrorResponse(
                    "validation_error",
                    string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The supplied value is invalid."
                        : error.ErrorMessage,
                    entry.Key)))
            .Distinct()
            .ToArray();

        return Create(errors);
    }

    private static ApiResponse<object?> Create(
        IReadOnlyCollection<ErrorResponse> errors)
    {
        return ApiResponse.Failure<object?>(
            400,
            ValidationMessage,
            errors);
    }
}
