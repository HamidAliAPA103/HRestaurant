using FluentValidation;
using HRestaurant.DTOS.Customer;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Validators.Customers;

public sealed class CustomerCreateDTOValidator : AbstractValidator<CustomerCreateDTO>
{
    public CustomerCreateDTOValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20)
            .Matches(@"^\+?[0-9\s()\-]{7,20}$");
        RuleFor(x => x.Email).EmailAddress().MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Birthday).Must(x => !x.HasValue || x <= DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public sealed class CustomerUpdateDTOValidator : AbstractValidator<CustomerUpdateDTO>
{
    public CustomerUpdateDTOValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(20)
            .Matches(@"^\+?[0-9\s()\-]{7,20}$");
        RuleFor(x => x.Email).EmailAddress().MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Birthday).Must(x => !x.HasValue || x <= DateOnly.FromDateTime(DateTime.UtcNow));
        RuleFor(x => x.Notes).MaximumLength(1000);
    }
}

public sealed class CustomerListRequestValidator : AbstractValidator<CustomerListRequest>
{
    public CustomerListRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationRequest.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.Phone).MaximumLength(20);
        RuleFor(x => x.Email).MaximumLength(254);
    }
}
