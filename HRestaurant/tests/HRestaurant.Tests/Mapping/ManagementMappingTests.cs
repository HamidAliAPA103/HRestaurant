using AutoMapper;
using HRestaurant.Mappings.Branches;
using HRestaurant.Mappings.Ingredients;
using HRestaurant.Mappings.Inventory;
using HRestaurant.Mappings.MenuCategories;
using HRestaurant.Mappings.Menus;
using HRestaurant.Mappings.Restaurants;
using HRestaurant.Mappings.Shifts;
using HRestaurant.Mappings.Suppliers;
using HRestaurant.Mappings.Tables;
using HRestaurant.Mappings.Users;
using Microsoft.Extensions.Logging.Abstractions;

namespace HRestaurant.Tests.Mapping;

public sealed class ManagementMappingTests
{
    [Fact]
    public void ManagementProfiles_AreValid()
    {
        var configuration = new MapperConfiguration(
            expression =>
            {
                expression.AddProfile<RestaurantProfile>();
                expression.AddProfile<BranchProfile>();
                expression.AddProfile<UserProfile>();
                expression.AddProfile<MenuCategoryProfile>();
                expression.AddProfile<MenuProfile>();
                expression.AddProfile<IngredientProfile>();
                expression.AddProfile<ShiftProfile>();
                expression.AddProfile<SupplierProfile>();
                expression.AddProfile<InventoryProfile>();
                expression.AddProfile<TableProfile>();
            },
            NullLoggerFactory.Instance);

        configuration.AssertConfigurationIsValid();
    }
}
