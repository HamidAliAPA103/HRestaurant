using FluentValidation;
using HRestaurant.DTOS.MenuCategory;
using HRestaurant.DTOS.Responses;
using HRestaurant.Validators.Common;

namespace HRestaurant.Validators.MenuCategories;

public sealed class MenuCategoryCreateDTOValidator
    : AbstractValidator<MenuCategoryCreateDTO>
{
    public MenuCategoryCreateDTOValidator()
    {
        RuleFor(x => x.ResdaranId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty()
            .MaximumLength(ValidationConstants.NameMaximumLength);
        RuleFor(x => x.Description).MaximumLength(1000)
            .When(x => x.Description is not null);
        RuleFor(x => x.ImageUrl).MaximumLength(500)
            .When(x => x.ImageUrl is not null);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class MenuCategoryUpdateDTOValidator
    : AbstractValidator<MenuCategoryUpdateDTO>
{
    public MenuCategoryUpdateDTOValidator()
    {
        RuleFor(x => x.Name).NotEmpty()
            .MaximumLength(ValidationConstants.NameMaximumLength)
            .When(x => x.Name is not null);
        RuleFor(x => x.Description).MaximumLength(1000)
            .When(x => x.Description is not null);
        RuleFor(x => x.ImageUrl).MaximumLength(500)
            .When(x => x.ImageUrl is not null);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0)
            .When(x => x.DisplayOrder.HasValue);
    }
}

public sealed class MenuCategoryListRequestValidator
    : AbstractValidator<MenuCategoryListRequest>
{
    public MenuCategoryListRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationRequest.MaxPageSize);
    }
}

public sealed class MenuCategoryDisplayOrderDTOValidator
    : AbstractValidator<MenuCategoryDisplayOrderDTO>
{
    public MenuCategoryDisplayOrderDTOValidator() =>
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
}
