using FluentValidation;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Supplier;
using HRestaurant.Validators.Common;

namespace HRestaurant.Validators.Suppliers;

public sealed class SupplierCreateDTOValidator : AbstractValidator<SupplierCreateDTO>
{
    public SupplierCreateDTOValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ContactPerson).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().Matches(ValidationConstants.PhonePattern).MaximumLength(20);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
    }
}

public sealed class SupplierUpdateDTOValidator : AbstractValidator<SupplierUpdateDTO>
{
    public SupplierUpdateDTOValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.ContactPerson).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().Matches(ValidationConstants.PhonePattern).MaximumLength(20);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(254);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(300);
    }
}

public sealed class SupplierListRequestValidator : AbstractValidator<SupplierListRequest>
{
    public SupplierListRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationRequest.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(150).When(x => x.Search is not null);
    }
}
