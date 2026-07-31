using FluentValidation;
using HRestaurant.DTOS.Ingredient;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Validators.Ingredients;

public sealed class IngredientCreateDTOValidator
    : AbstractValidator<IngredientCreateDTO>
{
    public IngredientCreateDTOValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Unit).IsInEnum();
    }
}

public sealed class IngredientUpdateDTOValidator
    : AbstractValidator<IngredientUpdateDTO>
{
    public IngredientUpdateDTOValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Unit).IsInEnum();
    }
}

public sealed class IngredientListRequestValidator
    : AbstractValidator<IngredientListRequest>
{
    public IngredientListRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationRequest.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(100)
            .When(x => x.Search is not null);
    }
}
