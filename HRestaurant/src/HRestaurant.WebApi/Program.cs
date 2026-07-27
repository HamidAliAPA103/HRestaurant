using AutoMapper;
using FluentValidation;
using HRestaurant.Infrastructure;
using HRestaurant.Mappings.Restaurants;
using HRestaurant.Validators.Restaurants;
using HRestaurant.WebApi.Validation;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddValidatorsFromAssemblyContaining<
    RestaurantCreateDTOValidator>();
builder.Services.AddScoped<FluentValidationActionFilter>();

builder.Services
    .AddControllers(options =>
        options.Filters.AddService<FluentValidationActionFilter>())
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
            new BadRequestObjectResult(
                ValidationErrorResponseFactory.FromModelState(
                    context.ModelState));
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var autoMapperLicenseKey =
    builder.Configuration["AutoMapper:LicenseKey"];

builder.Services.AddAutoMapper(
    configuration =>
    {
        if (!string.IsNullOrWhiteSpace(autoMapperLicenseKey))
        {
            configuration.LicenseKey = autoMapperLicenseKey;
        }
    },
    typeof(RestaurantProfile));
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.Services
        .GetRequiredService<IMapper>()
        .ConfigurationProvider
        .AssertConfigurationIsValid();

    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAuthorization();

app.MapControllers();

app.Run();
