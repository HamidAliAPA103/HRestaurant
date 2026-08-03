using System.Text.Json;
using AutoMapper;
using FluentAssertions;
using HRestaurant.Data;
using HRestaurant.Enum;
using HRestaurant.Models;
using HRestaurant.Services.Implementations;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace HRestaurant.Tests.Services;

public sealed class PublicMenu3DServiceTests
{
    [Fact]
    public async Task Public3DEndpoints_ReturnPresentationData_WithoutPrivateInventoryData()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;
        await using var dbContext = new AppDbContext(options);
        await dbContext.Database.EnsureCreatedAsync();

        var restaurantId = Guid.NewGuid();
        var categoryId = Guid.NewGuid();
        var menuItemId = Guid.NewGuid();
        var ingredientId = Guid.NewGuid();
        dbContext.Restaurants.Add(new Restaurant
        {
            ID = restaurantId,
            Name = "Public Test Restaurant",
            Slug = "public-test",
            Adres = "Baku",
            Number = "+994501112233",
            IsActive = true
        });
        dbContext.MenuCategories.Add(new MenuCategory
        {
            ID = categoryId,
            ResdaranId = restaurantId,
            Name = "Main",
            NormalizedName = "MAIN",
            IsActive = true
        });
        dbContext.Menus.Add(new Menu
        {
            ID = menuItemId,
            RestaurantId = restaurantId,
            CategoryId = categoryId,
            Name = "Test Burger",
            NormalizedName = "TEST BURGER",
            Desc = "A public description",
            Nutrition = "Nutrition summary",
            Image = string.Empty,
            ImageURL = "https://cdn.example/food.jpg",
            Price = 12m,
            FinalPrice = 10m,
            DiscountPercentage = 16.67m,
            PreparationTimeMinutes = 15,
            IsAvailable = true,
            Is3DEnabled = true,
            Model3DUrl = "https://cdn.example/food.glb",
            ModelPosterUrl = "https://cdn.example/poster.jpg",
            ModelScale = 1.2m
        });
        dbContext.Ingredients.Add(new Ingredient
        {
            ID = ingredientId,
            RestaurantId = restaurantId,
            Name = "Pomidor",
            NormalizedName = "POMIDOR",
            Unit = IngredientUnit.Gram,
            IsActive = true,
            Description = "Fresh sliced tomato",
            Calories = 18m,
            Origin = "Azerbaijan",
            AllergenInformation = "None declared"
        });
        dbContext.MenuItemIngredients.Add(new MenuItemIngredient
        {
            MenuItemId = menuItemId,
            IngredientId = ingredientId,
            RequiredQuantity = 40m,
            ExplodedPositionX = 2m,
            ExplodedPositionY = 1m,
            DisplayOrder = 1,
            IsVisibleIn3D = true
        });
        await dbContext.SaveChangesAsync();

        var service = new PublicRestaurantService(
            dbContext,
            Mock.Of<IMapper>(),
            TimeProvider.System);

        var foodResponse = await service.GetMenuItem3DAsync(menuItemId);
        var ingredientResponse = await service.GetMenuItemIngredients3DAsync(menuItemId);

        foodResponse.Data.Should().NotBeNull();
        foodResponse.Data!.RestaurantSlug.Should().Be("public-test");
        foodResponse.Data.ModelScale.Should().Be(1.2m);
        foodResponse.Data.UsesProceduralFallback.Should().BeFalse();

        ingredientResponse.Data.Should().ContainSingle();
        var ingredient = ingredientResponse.Data!.Single();
        ingredient.Name.Should().Be("Pomidor");
        ingredient.FallbackKind.Should().Be("tomato");
        ingredient.RequiredQuantity.Should().Be(40m);
        ingredient.ExplodedPositionX.Should().Be(2m);

        var publicJson = JsonSerializer.Serialize(ingredientResponse.Data);
        publicJson.Should().NotContain("Supplier");
        publicJson.Should().NotContain("PurchasePrice");
        publicJson.Should().NotContain("Inventory");
    }
}
