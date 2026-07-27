using FluentValidation.Results;
using HRestaurant.DTOS.Responses;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace HRestaurant.WebApi.Validation;

internal static class ValidationErrorResponseFactory
{
    public static ValidationErrorResponse FromFailures(
        IEnumerable<ValidationFailure> failures)
    {
        ArgumentNullException.ThrowIfNull(failures);

        var errors = failures
            .Where(failure => failure is not null)
            .Select(failure => new ValidationErrorDTO(
                failure.PropertyName,
                failure.ErrorMessage))
            .Distinct()
            .ToArray();

        return Create(errors);
    }

    public static ValidationErrorResponse FromModelState(
        ModelStateDictionary modelState)
    {
        ArgumentNullException.ThrowIfNull(modelState);

        var errors = modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .SelectMany(entry => entry.Value!.Errors.Select(error =>
                new ValidationErrorDTO(
                    entry.Key,
                    string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The supplied value is invalid."
                        : error.ErrorMessage)))
            .Distinct()
            .ToArray();

        return Create(errors);
    }

    private static ValidationErrorResponse Create(
        IReadOnlyCollection<ValidationErrorDTO> errors)
    {
        return new ValidationErrorResponse
        {
            Errors = errors
        };
    }
}
