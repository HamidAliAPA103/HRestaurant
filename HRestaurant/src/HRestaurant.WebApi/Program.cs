using AutoMapper;
using FluentValidation;
using HRestaurant.Infrastructure;
using HRestaurant.Mappings.Restaurants;
using HRestaurant.Validators.Restaurants;
using HRestaurant.WebApi.ExceptionHandling;
using HRestaurant.WebApi.Validation;
using Microsoft.AspNetCore.Mvc;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) =>
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext());

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
            {
                var response =
                    ValidationErrorResponseFactory.FromModelState(
                        context.ModelState,
                        context.HttpContext.TraceIdentifier);

                return new ObjectResult(response)
                {
                    StatusCode = response.StatusCode
                };
            };
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

    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set(
                "TraceId",
                httpContext.TraceIdentifier);
            diagnosticContext.Set(
                "RequestHost",
                httpContext.Request.Host.Value);
            diagnosticContext.Set(
                "RequestScheme",
                httpContext.Request.Scheme);
        };
    });

    app.UseMiddleware<GlobalExceptionMiddleware>();

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
}
catch (Exception exception)
{
    Log.Fatal(exception, "Application terminated unexpectedly.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
