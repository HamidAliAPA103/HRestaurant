using HRestaurant.Data;
using HRestaurant.Services.Implaments;
using HRestaurant.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddScoped<IRestaurantService, RestaurantService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<IMenuCategoryService, MenuCategoryService>();
builder.Services.AddScoped<IOrderItemService, OrderItemService>();
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddScoped<IReviewService, ReviewService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IUserService, UserService>();


// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AppDbContext>(opt =>
opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
