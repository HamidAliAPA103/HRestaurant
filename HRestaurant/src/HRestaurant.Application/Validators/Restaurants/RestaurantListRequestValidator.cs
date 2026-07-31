using FluentValidation;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Restaurant;

namespace HRestaurant.Validators.Restaurants;

public sealed class RestaurantListRequestValidator
    : AbstractValidator<RestaurantListRequest>
{
    private static readonly string[] AllowedSortFields =
    [
        "name",
        "createdat"
    ];

    private static readonly string[] AllowedSortDirections =
    [
        "asc",
        "desc"
    ];

    public RestaurantListRequestValidator()
    {
        RuleFor(request => request.PageNumber)
            .GreaterThanOrEqualTo(1);

        RuleFor(request => request.PageSize)
            .InclusiveBetween(1, PaginationRequest.MaxPageSize);

        RuleFor(request => request.Search)
            .MaximumLength(100)
            .When(request => request.Search is not null);

        RuleFor(request => request.Type)
            .IsInEnum();

        RuleFor(request => request.SortBy)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(value => AllowedSortFields.Contains(
                value.Trim().ToLowerInvariant()))
            .WithMessage("SortBy must be 'name' or 'createdAt'.");

        RuleFor(request => request.SortDirection)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .Must(value => AllowedSortDirections.Contains(
                value.Trim().ToLowerInvariant()))
            .WithMessage("SortDirection must be 'asc' or 'desc'.");
    }
}
