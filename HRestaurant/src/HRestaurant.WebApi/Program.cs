using AutoMapper;
using HRestaurant.Infrastructure;
using HRestaurant.Mappings.Restaurants;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
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
