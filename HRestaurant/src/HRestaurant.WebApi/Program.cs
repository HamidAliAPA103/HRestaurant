using AutoMapper;
using FluentValidation;
using HRestaurant.Infrastructure;
using HRestaurant.Infrastructure.Identity;
using HRestaurant.Mappings.Restaurants;
using HRestaurant.Validators.Restaurants;
using HRestaurant.WebApi.ExceptionHandling;
using HRestaurant.WebApi.RateLimiting;
using HRestaurant.WebApi.Swagger;
using HRestaurant.WebApi.Validation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;

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
    builder.Services.AddPublicRateLimiting();
    var allowedOrigins =
        builder.Configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>()
        ?? [];

    builder.Services.AddCors(options =>
    {
        options.AddPolicy("Frontend", policy =>
        {
            if (allowedOrigins.Length > 0)
            {
                policy
                    .WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            }
        });
    });
    builder.Services.AddSwaggerGen(options =>
    {
        options.EnableAnnotations();
        options.OperationFilter<PublicApiExamplesOperationFilter>();

        const string bearerScheme = "Bearer";

        options.AddSecurityDefinition(
            bearerScheme,
            new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description =
                    "Enter a JWT access token using: Bearer {token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

        options.AddSecurityRequirement(
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = bearerScheme
                        }
                    }
                ] = Array.Empty<string>()
            });
    });
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

    await app.Services.SeedIdentityDataAsync();

    app.UseSerilogRequestLogging(options =>
    {
        options.GetLevel = (httpContext, _, exception) =>
        {
            if (httpContext.Request.Path.StartsWithSegments(
                    "/api/public/reservations/track",
                    StringComparison.OrdinalIgnoreCase))
            {
                return LogEventLevel.Verbose;
            }

            return exception is not null
                || httpContext.Response.StatusCode >= 500
                ? LogEventLevel.Error
                : LogEventLevel.Information;
        };

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
    app.UseRouting();
    app.UseCors("Frontend");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (HostAbortedException)
{
    throw;
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
