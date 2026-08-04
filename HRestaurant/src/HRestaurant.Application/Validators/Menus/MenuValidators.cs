using FluentValidation;
using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Responses;
using HRestaurant.Validators.Common;

namespace HRestaurant.Validators.Menus;

public sealed class MenuCreateDTOValidator : AbstractValidator<MenuCreateDTO>
{
    public MenuCreateDTOValidator()
    {
        RuleFor(x => x).Must(x => x.Image is not null
                || !string.IsNullOrWhiteSpace(x.ImageUrl))
            .WithMessage("An image file or image URL is required.");
        RuleFor(x => x.Image!.Length).GreaterThan(0)
            .When(x => x.Image is not null);
        RuleFor(x => x.ImageUrl).MaximumLength(500)
            .When(x => x.ImageUrl is not null);
        RuleFor(x => x.Model3DUrl).MaximumLength(500)
            .Must(PublicModelUrlRules.IsValid)
            .WithMessage("Model3DUrl must be an absolute HTTP or HTTPS URL.")
            .When(x => x.Model3DUrl is not null);
        RuleFor(x => x.ModelPosterUrl).MaximumLength(500)
            .Must(PublicModelUrlRules.IsValid)
            .WithMessage("ModelPosterUrl must be an absolute HTTP or HTTPS URL.")
            .When(x => x.ModelPosterUrl is not null);
        RuleFor(x => x.VideoUrl).MaximumLength(500)
            .Must(PublicModelUrlRules.IsValid)
            .WithMessage("VideoUrl must be an absolute HTTP or HTTPS URL.")
            .When(x => x.VideoUrl is not null);
        RuleFor(x => x.VideoPosterUrl).MaximumLength(500)
            .Must(PublicModelUrlRules.IsValid)
            .WithMessage("VideoPosterUrl must be an absolute HTTP or HTTPS URL.")
            .When(x => x.VideoPosterUrl is not null);
        RuleFor(x => x.VideoDurationSeconds).GreaterThanOrEqualTo(0)
            .When(x => x.VideoDurationSeconds.HasValue);
        RuleFor(x => x.VideoDisplayOrder).GreaterThanOrEqualTo(0);
        RuleFor(x => x).Must(x => !x.IsVideoEnabled || !string.IsNullOrWhiteSpace(x.VideoUrl))
            .WithMessage("VideoUrl is required when video presentation is enabled.");
        RuleFor(x => x.ModelScale).InclusiveBetween(0.01m, 100m);
        RuleFor(x => x.ModelRotationX).InclusiveBetween(-360m, 360m);
        RuleFor(x => x.ModelRotationY).InclusiveBetween(-360m, 360m);
        RuleFor(x => x.ModelRotationZ).InclusiveBetween(-360m, 360m);
        RuleFor(x => x).Must(x => !x.Is3DEnabled
                || !string.IsNullOrWhiteSpace(x.Model3DUrl))
            .WithMessage("Model3DUrl is required when 3D presentation is enabled.");
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThan(0);
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100);
        RuleFor(x => x.PreparationTimeMinutes).InclusiveBetween(1, 1440);
        RuleFor(x => x.Desc).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x.Nutrition).MaximumLength(1000);
        RuleFor(x => x.Ingredients).Must(MenuIngredientRules.Unique)
            .WithMessage("Duplicate ingredients are not allowed.");
        RuleForEach(x => x.Ingredients)
            .SetValidator(new MenuItemIngredientDTOValidator());
    }
}

public sealed class MenuUpdateDTOValidator : AbstractValidator<MenuUpdateDTO>
{
    public MenuUpdateDTOValidator()
    {
        RuleFor(x => x.Image!.Length).GreaterThan(0)
            .When(x => x.Image is not null);
        RuleFor(x => x.ImageURL).NotEmpty().MaximumLength(500)
            .When(x => x.ImageURL is not null);
        RuleFor(x => x.Model3DUrl).NotEmpty().MaximumLength(500)
            .Must(PublicModelUrlRules.IsValid)
            .WithMessage("Model3DUrl must be an absolute HTTP or HTTPS URL.")
            .When(x => x.Model3DUrl is not null);
        RuleFor(x => x.ModelPosterUrl).NotEmpty().MaximumLength(500)
            .Must(PublicModelUrlRules.IsValid)
            .WithMessage("ModelPosterUrl must be an absolute HTTP or HTTPS URL.")
            .When(x => x.ModelPosterUrl is not null);
        RuleFor(x => x.VideoUrl).NotEmpty().MaximumLength(500)
            .Must(PublicModelUrlRules.IsValid)
            .WithMessage("VideoUrl must be an absolute HTTP or HTTPS URL.")
            .When(x => x.VideoUrl is not null);
        RuleFor(x => x.VideoPosterUrl).NotEmpty().MaximumLength(500)
            .Must(PublicModelUrlRules.IsValid)
            .WithMessage("VideoPosterUrl must be an absolute HTTP or HTTPS URL.")
            .When(x => x.VideoPosterUrl is not null);
        RuleFor(x => x.VideoDurationSeconds).GreaterThanOrEqualTo(0)
            .When(x => x.VideoDurationSeconds.HasValue);
        RuleFor(x => x.VideoDisplayOrder).GreaterThanOrEqualTo(0)
            .When(x => x.VideoDisplayOrder.HasValue);
        RuleFor(x => x.ModelScale).InclusiveBetween(0.01m, 100m)
            .When(x => x.ModelScale.HasValue);
        RuleFor(x => x.ModelRotationX).InclusiveBetween(-360m, 360m)
            .When(x => x.ModelRotationX.HasValue);
        RuleFor(x => x.ModelRotationY).InclusiveBetween(-360m, 360m)
            .When(x => x.ModelRotationY.HasValue);
        RuleFor(x => x.ModelRotationZ).InclusiveBetween(-360m, 360m)
            .When(x => x.ModelRotationZ.HasValue);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100)
            .When(x => x.Name is not null);
        RuleFor(x => x.Price).GreaterThan(0).When(x => x.Price.HasValue);
        RuleFor(x => x.DiscountPercentage).InclusiveBetween(0, 100)
            .When(x => x.DiscountPercentage.HasValue);
        RuleFor(x => x.PreparationTimeMinutes).InclusiveBetween(1, 1440)
            .When(x => x.PreparationTimeMinutes.HasValue);
        RuleFor(x => x.CategoryId).NotEmpty().When(x => x.CategoryId.HasValue);
        RuleFor(x => x.Desc).NotEmpty().MaximumLength(1000)
            .When(x => x.Desc is not null);
        RuleFor(x => x.Nutrition).MaximumLength(1000)
            .When(x => x.Nutrition is not null);
        RuleFor(x => x.Ingredients!).Must(MenuIngredientRules.Unique)
            .When(x => x.Ingredients is not null)
            .WithMessage("Duplicate ingredients are not allowed.");
        RuleForEach(x => x.Ingredients!)
            .SetValidator(new MenuItemIngredientDTOValidator())
            .When(x => x.Ingredients is not null);
    }
}

public sealed class MenuItemIngredientDTOValidator
    : AbstractValidator<MenuItemIngredientDTO>
{
    public MenuItemIngredientDTOValidator()
    {
        RuleFor(x => x.IngredientId).NotEmpty();
        RuleFor(x => x.RequiredQuantity).GreaterThan(0);
        RuleFor(x => x.ExplodedPositionX).InclusiveBetween(-100m, 100m);
        RuleFor(x => x.ExplodedPositionY).InclusiveBetween(-100m, 100m);
        RuleFor(x => x.ExplodedPositionZ).InclusiveBetween(-100m, 100m);
        RuleFor(x => x.ExplodedRotationX).InclusiveBetween(-360m, 360m);
        RuleFor(x => x.ExplodedRotationY).InclusiveBetween(-360m, 360m);
        RuleFor(x => x.ExplodedRotationZ).InclusiveBetween(-360m, 360m);
        RuleFor(x => x.DisplayOrder).GreaterThanOrEqualTo(0);
    }
}

public sealed class MenuItemIngredientQuantityDTOValidator
    : AbstractValidator<MenuItemIngredientQuantityDTO>
{
    public MenuItemIngredientQuantityDTOValidator()
    {
        RuleFor(x => x.RequiredQuantity).GreaterThan(0);
    }
}

public sealed class MenuListRequestValidator : AbstractValidator<MenuListRequest>
{
    public MenuListRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, PaginationRequest.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(100)
            .When(x => x.Search is not null);
        RuleFor(x => x.MinPrice).GreaterThanOrEqualTo(0)
            .When(x => x.MinPrice.HasValue);
        RuleFor(x => x.MaxPrice).GreaterThanOrEqualTo(0)
            .When(x => x.MaxPrice.HasValue);
        RuleFor(x => x).Must(x => !x.MinPrice.HasValue || !x.MaxPrice.HasValue
                || x.MinPrice <= x.MaxPrice)
            .WithMessage("MinPrice cannot exceed MaxPrice.");
        RuleFor(x => x.SortBy).Cascade(CascadeMode.Stop).NotEmpty()
            .Must(x => x.Equals("name", StringComparison.OrdinalIgnoreCase)
                || x.Equals("price", StringComparison.OrdinalIgnoreCase));
        RuleFor(x => x.SortDirection).Cascade(CascadeMode.Stop).NotEmpty()
            .Must(x => x.Equals("asc", StringComparison.OrdinalIgnoreCase)
                || x.Equals("desc", StringComparison.OrdinalIgnoreCase));
    }
}

internal static class MenuIngredientRules
{
    public static bool Unique(IEnumerable<MenuItemIngredientDTO>? items)
    {
        if (items is null) return true;
        var ids = items.Select(x => x.IngredientId).ToArray();
        return ids.Length == ids.Distinct().Count();
    }
}

internal static class PublicModelUrlRules
{
    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
