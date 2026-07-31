using System.IdentityModel.Tokens.Jwt;
using System.Text;
using HRestaurant.Configuration;
using HRestaurant.Data;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Infrastructure.BackgroundJobs;
using HRestaurant.Infrastructure.Identity;
using HRestaurant.Repositories.Implementations;
using HRestaurant.Repositories.Interfaces;
using HRestaurant.Services.Implementations;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

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

        AddIdentity(services);
        AddJwtAuthentication(services, configuration);
        AddPublicReservationServices(services, configuration);

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IRestaurantService, RestaurantService>();
        services.AddScoped<IBranchService, BranchService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IMenuService, MenuService>();
        services.AddScoped<IMenuCategoryService, MenuCategoryService>();
        services.AddScoped<IOrderItemService, OrderItemService>();
        services.AddScoped<IReservationService, ReservationService>();
        services.AddScoped<IReviewService, ReviewService>();
        services.AddScoped<ITableService, TableService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<ICurrentUserContext, CurrentUserContext>();

        services.AddHttpContextAccessor();

        return services;
    }

    private static void AddPublicReservationServices(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var publicSettings = configuration
            .GetSection(PublicReservationSettings.SectionName)
            .Get<PublicReservationSettings>()
            ?? new PublicReservationSettings();
        ValidatePublicReservationSettings(publicSettings);
        services.AddSingleton(publicSettings);

        var emailSettings = configuration
            .GetSection(ReservationEmailSettings.SectionName)
            .Get<ReservationEmailSettings>()
            ?? new ReservationEmailSettings();
        ValidateEmailSettings(emailSettings);
        services.AddSingleton(emailSettings);

        services.AddScoped<
            IPublicRestaurantService,
            PublicRestaurantService>();
        services.AddScoped<
            ITableAvailabilityService,
            TableAvailabilityService>();
        services.AddScoped<
            IPublicReservationService,
            PublicReservationService>();
        services.AddSingleton<
            IReservationConfirmationService,
            ReservationConfirmationService>();
        services.AddSingleton<
            IPublicRequestChallengeValidator,
            DisabledPublicRequestChallengeValidator>();

        services.AddSingleton<ReservationEmailQueue>();
        services.AddSingleton<IReservationEmailQueue>(
            provider => provider
                .GetRequiredService<ReservationEmailQueue>());
        services.AddScoped<
            IReservationEmailSender,
            SmtpReservationEmailSender>();
        services.AddHostedService<
            ReservationEmailBackgroundService>();
    }

    private static void AddIdentity(IServiceCollection services)
    {
        services
            .AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;

                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 1;

                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);
            })
            .AddRoles<AppRole>()
            .AddSignInManager()
            .AddEntityFrameworkStores<AppDbContext>();
    }

    private static void AddJwtAuthentication(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection(
            JwtSettings.SectionName);
        var settings = jwtSection.Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{JwtSettings.SectionName}' was not found.");

        ValidateJwtSettings(settings);
        services.Configure<JwtSettings>(jwtSection);

        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(settings.Key));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                    JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.MapInboundClaims = false;
                options.SaveToken = false;
                options.TokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = settings.Issuer,
                        ValidateAudience = true,
                        ValidAudience = settings.Audience,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,
                        ValidateLifetime = true,
                        RequireExpirationTime = true,
                        RequireSignedTokens = true,
                        ClockSkew = TimeSpan.Zero,
                        NameClaimType = JwtRegisteredClaimNames.Email,
                        RoleClaimType = AuthClaimTypes.Role
                    };

                options.Events = new JwtBearerEvents
                {
                    OnChallenge = WriteUnauthorizedResponseAsync,
                    OnForbidden = WriteForbiddenResponseAsync
                };
            });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            options.AddPolicy(
                AuthorizationPolicies.EmployeeManagement,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(
                        AppRoles.SuperAdmin,
                        AppRoles.RestaurantOwner,
                        AppRoles.Manager));

            options.AddPolicy(
                AuthorizationPolicies.PaymentProcessing,
                policy => policy
                    .RequireAuthenticatedUser()
                    .RequireRole(
                        AppRoles.SuperAdmin,
                        AppRoles.Cashier,
                        AppRoles.Manager));
        });

        services.AddSingleton<
            IAuthorizationPolicyProvider,
            PermissionAuthorizationPolicyProvider>();
        services.AddSingleton<
            IAuthorizationHandler,
            PermissionHandler>();
    }

    private static Task WriteUnauthorizedResponseAsync(
        JwtBearerChallengeContext context)
    {
        context.HandleResponse();

        return WriteAuthenticationFailureAsync(
            context.HttpContext,
            StatusCodes.Status401Unauthorized,
            "unauthorized",
            "A valid access token is required.");
    }

    private static Task WriteForbiddenResponseAsync(
        ForbiddenContext context)
    {
        return WriteAuthenticationFailureAsync(
            context.HttpContext,
            StatusCodes.Status403Forbidden,
            "forbidden",
            "You do not have permission to access this resource.");
    }

    private static async Task WriteAuthenticationFailureAsync(
        HttpContext httpContext,
        int statusCode,
        string errorCode,
        string message)
    {
        if (httpContext.Response.HasStarted)
        {
            return;
        }

        var response = ApiResponse.Failure<object?>(
            statusCode,
            message,
            [new ErrorResponse(errorCode, message)],
            httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.Headers.CacheControl = "no-store";

        await httpContext.Response.WriteAsJsonAsync(
            response,
            cancellationToken: httpContext.RequestAborted);
    }

    private static void ValidateJwtSettings(JwtSettings settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Issuer))
        {
            throw new InvalidOperationException(
                "Jwt:Issuer must be configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Audience))
        {
            throw new InvalidOperationException(
                "Jwt:Audience must be configured.");
        }

        if (Encoding.UTF8.GetByteCount(settings.Key) < 32)
        {
            throw new InvalidOperationException(
                "Jwt:Key must contain at least 32 bytes.");
        }

        if (settings.AccessTokenMinutes is < 1 or > 1440)
        {
            throw new InvalidOperationException(
                "Jwt:AccessTokenMinutes must be between 1 and 1440.");
        }

        if (settings.RefreshTokenDays is < 1 or > 365)
        {
            throw new InvalidOperationException(
                "Jwt:RefreshTokenDays must be between 1 and 365.");
        }
    }

    private static void ValidatePublicReservationSettings(
        PublicReservationSettings settings)
    {
        if (settings.MinimumDurationMinutes <= 0
            || settings.MaximumDurationMinutes
            < settings.MinimumDurationMinutes)
        {
            throw new InvalidOperationException(
                "Public reservation duration settings are invalid.");
        }

        if (settings.DefaultDurationMinutes
                < settings.MinimumDurationMinutes
            || settings.DefaultDurationMinutes
                > settings.MaximumDurationMinutes)
        {
            throw new InvalidOperationException(
                "PublicReservations:DefaultDurationMinutes is invalid.");
        }

        if (settings.SlotIntervalMinutes <= 0
            || settings.BufferMinutes < 0
            || settings.MinimumGuestCount <= 0
            || settings.MaximumGuestCount
                < settings.MinimumGuestCount
            || settings.CancellationCutoffMinutes < 0)
        {
            throw new InvalidOperationException(
                "Public reservation limits are invalid.");
        }

        if (settings.InitialStatus is not (
                ReservationStatus.Pending
                or ReservationStatus.Confirmed))
        {
            throw new InvalidOperationException(
                "PublicReservations:InitialStatus must be "
                + "Pending or Confirmed.");
        }

        if (!Uri.TryCreate(
                settings.PublicBaseUrl,
                UriKind.Absolute,
                out _))
        {
            throw new InvalidOperationException(
                "PublicReservations:PublicBaseUrl must be an "
                + "absolute URL.");
        }
    }

    private static void ValidateEmailSettings(
        ReservationEmailSettings settings)
    {
        if (!settings.Enabled)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(settings.Host)
            || settings.Port is < 1 or > 65535
            || string.IsNullOrWhiteSpace(settings.FromAddress))
        {
            throw new InvalidOperationException(
                "Reservation email settings are invalid.");
        }
    }
}
