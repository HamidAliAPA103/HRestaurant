using HRestaurant.Data;
using HRestaurant.Repositories.Implementations;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Implementations;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace HRestaurant.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Connection string 'Default' was not found.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(
                    typeof(AppDbContext).Assembly.FullName)));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IMenuCategoryService, MenuCategoryService>();
        services.AddScoped<IOrderItemService, OrderItemService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IUserService, UserService>();

        services.AddHttpContextAccessor();

        return services;
    }
}
