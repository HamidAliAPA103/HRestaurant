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
        RuleFor(x => x.Model3DUrl).MaximumLength(500);
        RuleFor(x => x.ImageUrl).MaximumLength(500);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.Origin).MaximumLength(120);
        RuleFor(x => x.AllergenInformation).MaximumLength(500);
        RuleFor(x => x.Calories).GreaterThanOrEqualTo(0).When(x => x.Calories.HasValue);
        RuleFor(x => x.Protein).GreaterThanOrEqualTo(0).When(x => x.Protein.HasValue);
        RuleFor(x => x.Carbohydrates).GreaterThanOrEqualTo(0).When(x => x.Carbohydrates.HasValue);
        RuleFor(x => x.Fat).GreaterThanOrEqualTo(0).When(x => x.Fat.HasValue);
    }
}

public sealed class IngredientUpdateDTOValidator
    : AbstractValidator<IngredientUpdateDTO>
{
    public IngredientUpdateDTOValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Unit).IsInEnum();
        AddPublicPresentationRules(this);
    }

    private static void AddPublicPresentationRules(
        AbstractValidator<IngredientUpdateDTO> validator)
    {
        validator.RuleFor(x => x.Model3DUrl).MaximumLength(500);
        validator.RuleFor(x => x.ImageUrl).MaximumLength(500);
        validator.RuleFor(x => x.Description).MaximumLength(1000);
        validator.RuleFor(x => x.Origin).MaximumLength(120);
        validator.RuleFor(x => x.AllergenInformation).MaximumLength(500);
        validator.RuleFor(x => x.Calories).GreaterThanOrEqualTo(0).When(x => x.Calories.HasValue);
        validator.RuleFor(x => x.Protein).GreaterThanOrEqualTo(0).When(x => x.Protein.HasValue);
        validator.RuleFor(x => x.Carbohydrates).GreaterThanOrEqualTo(0).When(x => x.Carbohydrates.HasValue);
        validator.RuleFor(x => x.Fat).GreaterThanOrEqualTo(0).When(x => x.Fat.HasValue);
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
