using FluentAssertions;
using HRestaurant.DTOS.Common;
using HRestaurant.DTOS.Menu;
using HRestaurant.DTOS.Shift;
using HRestaurant.DTOS.User;
using HRestaurant.Validators.Menus;
using HRestaurant.Validators.Shifts;
using HRestaurant.Validators.Users;

namespace HRestaurant.Tests.Validation;

public sealed class EmployeeShiftMenuValidationTests
{
    [Fact]
    public void MenuCreate_RejectsInvalidPriceAndDiscount()
    {
        var dto = ValidMenu();
        dto.Price = 0;
        dto.DiscountPercentage = 101;

        var result = new MenuCreateDTOValidator().Validate(dto);

        result.IsValid.Should().BeFalse();
        result.Errors.Select(x => x.PropertyName)
            .Should().Contain([nameof(dto.Price), nameof(dto.DiscountPercentage)]);
    }

    [Fact]
    public void MenuCreate_RejectsDuplicateIngredients()
    {
        var dto = ValidMenu();
        var ingredientId = Guid.NewGuid();
        dto.Ingredients =
        [
            new() { IngredientId = ingredientId, RequiredQuantity = 100 },
            new() { IngredientId = ingredientId, RequiredQuantity = 50 }
        ];

        new MenuCreateDTOValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void ShiftCreate_RejectsEndTimeBeforeStartTime()
    {
        var dto = new ShiftCreateDTO
        {
            RestaurantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Name = "Morning",
            StartTime = new TimeOnly(17, 0),
            EndTime = new TimeOnly(9, 0)
        };

        new ShiftCreateDTOValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void EmployeeShiftAssignment_RejectsInvalidCustomRange()
    {
        var dto = new EmployeeShiftAssignDTO
        {
            EmployeeId = Guid.NewGuid(),
            ShiftId = Guid.NewGuid(),
            WorkDate = new DateOnly(2026, 8, 3),
            StartTime = new TimeOnly(12, 0),
            EndTime = new TimeOnly(11, 0)
        };

        new EmployeeShiftAssignDTOValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    [Fact]
    public void EmployeeCreate_RejectsUnsupportedRole()
    {
        var dto = new UserCreateDTO
        {
            RestaurantId = Guid.NewGuid(),
            BranchId = Guid.NewGuid(),
            Email = "employee@example.com",
            Name = "Employee Name",
            Phone = "+994501234567",
            Role = "RestaurantOwner",
            Salary = 1000,
            HireDate = new DateOnly(2026, 7, 31),
            EmergencyContact = "+994551234567",
            Password = "Strong!Pass1"
        };

        new UserCreateDTOValidator().Validate(dto).IsValid.Should().BeFalse();
    }

    private static MenuCreateDTO ValidMenu() => new()
    {
        Image = new FileUploadDTO
        {
            Content = Stream.Null,
            FileName = "menu.jpg",
            ContentType = "image/jpeg",
            Length = 100
        },
        Name = "Menu item",
        Price = 10,
        DiscountPercentage = 10,
        PreparationTimeMinutes = 15,
        Desc = "Description",
        CategoryId = Guid.NewGuid()
    };
}
