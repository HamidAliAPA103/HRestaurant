using FluentValidation;
using HRestaurant.DTOS.Review;
using HRestaurant.Validators.Common;

namespace HRestaurant.Validators.Reviews;

public sealed class ReviewCreateDTOValidator
    : AbstractValidator<ReviewCreateDTO>
{
    public ReviewCreateDTOValidator()
    {
        RuleFor(dto => dto.CustomerId)
            .NotEmpty()
            .WithMessage("Customer id is required.");

        RuleFor(dto => dto.ResdaranId)
            .NotEmpty()
            .WithMessage("Restaurant id is required.");

        RuleFor(dto => dto.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(dto => dto.Comment)
            .MaximumLength(ValidationConstants.CommentMaximumLength)
            .WithMessage(
                $"Comment cannot exceed {ValidationConstants.CommentMaximumLength} characters.")
            .When(dto => dto.Comment is not null);
    }
}

public sealed class ReviewUpdateDTOValidator
    : AbstractValidator<ReviewUpdateDTO>
{
    public ReviewUpdateDTOValidator()
    {
        RuleFor(dto => dto.CustomerId)
            .NotEmpty()
            .WithMessage("Customer id is required.");

        RuleFor(dto => dto.ResdaranId)
            .NotEmpty()
            .WithMessage("Restaurant id is required.");

        RuleFor(dto => dto.Rating)
            .InclusiveBetween(1, 5)
            .WithMessage("Rating must be between 1 and 5.");

        RuleFor(dto => dto.Comment)
            .MaximumLength(ValidationConstants.CommentMaximumLength)
            .WithMessage(
                $"Comment cannot exceed {ValidationConstants.CommentMaximumLength} characters.")
            .When(dto => dto.Comment is not null);
    }
}
