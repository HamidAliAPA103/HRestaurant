using FluentValidation;
using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Table;

namespace HRestaurant.Validators.Tables;

public sealed class TableCreateDTOValidator : AbstractValidator<TableCreateDTO>
{
    public TableCreateDTOValidator()
    {
        RuleFor(x => x.RestaurantId).NotEmpty();
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.TableNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Shape).IsInEnum();
        Include(new TableGeometryValidator<TableCreateDTO>(
            x => x.PositionX, x => x.PositionY, x => x.PositionZ,
            x => x.RotationX, x => x.RotationY, x => x.RotationZ,
            x => x.Width, x => x.Length, x => x.Height));
    }
}

public sealed class TableUpdateDTOValidator : AbstractValidator<TableUpdateDTO>
{
    public TableUpdateDTOValidator()
    {
        RuleFor(x => x.TableNumber).NotEmpty().MaximumLength(30);
        RuleFor(x => x.Capacity).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Shape).IsInEnum();
        Include(new TableGeometryValidator<TableUpdateDTO>(
            x => x.PositionX, x => x.PositionY, x => x.PositionZ,
            x => x.RotationX, x => x.RotationY, x => x.RotationZ,
            x => x.Width, x => x.Length, x => x.Height));
    }
}

public sealed class TableListRequestValidator : AbstractValidator<TableListRequest>
{
    public TableListRequestValidator()
    {
        RuleFor(x => x.PageNumber).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PaginationRequest.MaxPageSize);
        RuleFor(x => x.Search).MaximumLength(30).When(x => x.Search is not null);
        RuleFor(x => x.Status).IsInEnum().When(x => x.Status.HasValue);
    }
}

public sealed class TableStatusUpdateDTOValidator : AbstractValidator<TableStatusUpdateDTO>
{
    public TableStatusUpdateDTOValidator() => RuleFor(x => x.Status).IsInEnum();
}

public sealed class TablePositionUpdateDTOValidator : AbstractValidator<TablePositionUpdateDTO>
{
    public TablePositionUpdateDTOValidator()
    {
        RuleFor(x => x).Must(x =>
                double.IsFinite(x.PositionX)
                && double.IsFinite(x.PositionY)
                && double.IsFinite(x.PositionZ))
            .WithMessage("Position values must be finite numbers.");
    }
}

public sealed class TableRotationUpdateDTOValidator : AbstractValidator<TableRotationUpdateDTO>
{
    public TableRotationUpdateDTOValidator()
    {
        RuleFor(x => x).Must(x =>
                double.IsFinite(x.RotationX)
                && double.IsFinite(x.RotationY)
                && double.IsFinite(x.RotationZ))
            .WithMessage("Rotation values must be finite numbers.");
    }
}

public sealed class TableSizeUpdateDTOValidator : AbstractValidator<TableSizeUpdateDTO>
{
    public TableSizeUpdateDTOValidator()
    {
        RuleFor(x => x.Width).InclusiveBetween(0.5, 10);
        RuleFor(x => x.Length).InclusiveBetween(0.5, 10);
        RuleFor(x => x.Height).InclusiveBetween(0.3, 3);
    }
}

public sealed class TableLayoutBulkUpdateDTOValidator
    : AbstractValidator<TableLayoutBulkUpdateDTO>
{
    public TableLayoutBulkUpdateDTOValidator()
    {
        RuleFor(x => x.BranchId).NotEmpty();
        RuleFor(x => x.Tables).NotEmpty().Must(items =>
            items.Select(i => i.TableId).Distinct().Count() == items.Count)
            .WithMessage("Duplicate table ids are not allowed.");
        RuleForEach(x => x.Tables).SetValidator(new TableLayoutItemDTOValidator());
    }
}

public sealed class TableLayoutItemDTOValidator : AbstractValidator<TableLayoutItemDTO>
{
    public TableLayoutItemDTOValidator()
    {
        RuleFor(x => x.TableId).NotEmpty();
        RuleFor(x => x).Must(x =>
                double.IsFinite(x.PositionX)
                && double.IsFinite(x.PositionY)
                && double.IsFinite(x.PositionZ)
                && double.IsFinite(x.RotationX)
                && double.IsFinite(x.RotationY)
                && double.IsFinite(x.RotationZ))
            .WithMessage("Position and rotation values must be finite numbers.");
        RuleFor(x => x.Width).InclusiveBetween(0.5, 10);
        RuleFor(x => x.Length).InclusiveBetween(0.5, 10);
    }
}

internal sealed class TableGeometryValidator<T> : AbstractValidator<T>
{
    public TableGeometryValidator(
        Func<T, double> x, Func<T, double> y, Func<T, double> z,
        Func<T, double> rx, Func<T, double> ry, Func<T, double> rz,
        Func<T, double> width, Func<T, double> length, Func<T, double> height)
    {
        RuleFor(value => value).Must(value =>
                double.IsFinite(x(value))
                && double.IsFinite(y(value))
                && double.IsFinite(z(value))
                && double.IsFinite(rx(value))
                && double.IsFinite(ry(value))
                && double.IsFinite(rz(value)))
            .WithMessage("Position and rotation values must be finite numbers.");
        RuleFor(value => value).Must(value => width(value) is >= 0.5 and <= 10)
            .WithMessage("Width must be between 0.5 and 10.");
        RuleFor(value => value).Must(value => length(value) is >= 0.5 and <= 10)
            .WithMessage("Length must be between 0.5 and 10.");
        RuleFor(value => value).Must(value => height(value) is >= 0.3 and <= 3)
            .WithMessage("Height must be between 0.3 and 3.");
    }
}
