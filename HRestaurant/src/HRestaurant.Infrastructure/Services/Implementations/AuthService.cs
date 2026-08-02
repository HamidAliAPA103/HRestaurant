using System.Data;
using System.Security.Claims;
using HRestaurant.Configuration;
using HRestaurant.Data;
using HRestaurant.DTOS.Auth;
using HRestaurant.DTOS.Responses;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Identity;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HRestaurant.Services.Implementations;

public sealed class AuthService : IAuthService
{
    private const string DefaultRole = AppRoles.RestaurantOwner;
    private const string InvalidCredentialsMessage =
        "Email or password is invalid.";
    private const string InvalidRefreshTokenMessage =
        "Refresh token is invalid or expired.";

    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly AppDbContext _dbContext;
    private readonly ITokenService _tokenService;
    private readonly TimeProvider _timeProvider;
    private readonly ICurrentUserContext _currentUser;
    private readonly IAccountEmailSender _accountEmailSender;
    private readonly PublicReservationSettings _publicSettings;

    public AuthService(
        UserManager<AppUser> userManager,
        RoleManager<AppRole> roleManager,
        SignInManager<AppUser> signInManager,
        AppDbContext dbContext,
        ITokenService tokenService,
        TimeProvider timeProvider,
        ICurrentUserContext currentUser,
        IAccountEmailSender accountEmailSender,
        PublicReservationSettings publicSettings)
    {
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(roleManager);
        ArgumentNullException.ThrowIfNull(signInManager);
        ArgumentNullException.ThrowIfNull(dbContext);
        ArgumentNullException.ThrowIfNull(tokenService);
        ArgumentNullException.ThrowIfNull(timeProvider);
        ArgumentNullException.ThrowIfNull(currentUser);
        ArgumentNullException.ThrowIfNull(accountEmailSender);
        ArgumentNullException.ThrowIfNull(publicSettings);

        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
        _dbContext = dbContext;
        _tokenService = tokenService;
        _timeProvider = timeProvider;
        _currentUser = currentUser;
        _accountEmailSender = accountEmailSender;
        _publicSettings = publicSettings;
    }

    public async Task<ApiResponse<AuthResponse>> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = request.Email.Trim();
        var existingUser = await _userManager.FindByEmailAsync(email);

        if (existingUser is not null)
        {
            const string message =
                "An account with this email already exists.";

            return ApiResponse.Failure<AuthResponse>(
                StatusCodes.Status409Conflict,
                message,
                [new ErrorResponse("duplicate_email", message, "Email")]);
        }

        var restaurantExists = await _dbContext.Restaurants
            .AsNoTracking()
            .AnyAsync(
                restaurant =>
                    restaurant.ID == request.RestaurantId
                    && !restaurant.IsDeleted,
                cancellationToken);

        if (!restaurantExists)
        {
            return ApiResponse.NotFound<AuthResponse>("Restaurant");
        }

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        var restaurantAlreadyClaimed = await _userManager.Users
            .AsNoTracking()
            .AnyAsync(
                user => user.RestaurantId == request.RestaurantId,
                cancellationToken);
        if (restaurantAlreadyClaimed)
        {
            const string message =
                "This restaurant already has an account. Ask a SuperAdmin or the restaurant owner to invite you.";
            return ApiResponse.Failure<AuthResponse>(
                StatusCodes.Status409Conflict,
                message,
                [new ErrorResponse("restaurant_already_claimed", message, "RestaurantId")]);
        }

        var roleResult = await EnsureDefaultRoleAsync();

        if (!roleResult.Succeeded)
        {
            return IdentityFailure(
                roleResult,
                StatusCodes.Status500InternalServerError,
                "The default role could not be initialized.");
        }

        var user = new AppUser
        {
            Id = Guid.NewGuid(),
            UserName = email,
            Email = email,
            FullName = request.FullName.Trim(),
            RestaurantId = request.RestaurantId,
            CreatedAtUtc = UtcNow
        };

        var createResult = await _userManager.CreateAsync(
            user,
            request.Password);

        if (!createResult.Succeeded)
        {
            return IdentityFailure(
                createResult,
                StatusCodes.Status400BadRequest,
                "Registration failed.");
        }

        var addToRoleResult = await _userManager.AddToRoleAsync(
            user,
            DefaultRole);

        if (!addToRoleResult.Succeeded)
        {
            return IdentityFailure(
                addToRoleResult,
                StatusCodes.Status500InternalServerError,
                "The user role could not be assigned.");
        }

        var response = await CreateTokenPairAsync(
            user,
            [DefaultRole]);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await SendVerificationEmailAsync(user, cancellationToken);

        return ApiResponse.Created(
            response,
            "Registration completed successfully.");
    }

    public async Task<ApiResponse<AuthResponse>> LoginAsync(
        LoginRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _userManager.FindByEmailAsync(
            request.Email.Trim());

        if (user is null)
        {
            return InvalidCredentials();
        }

        if (!await IsEmployeeAccountEnabledAsync(user.Id, cancellationToken))
        {
            return InvalidCredentials();
        }

        var signInResult =
            await _signInManager.CheckPasswordSignInAsync(
                user,
                request.Password,
                lockoutOnFailure: true);

        if (!signInResult.Succeeded)
        {
            return InvalidCredentials();
        }

        var roles = await _userManager.GetRolesAsync(user);
        var response = await CreateTokenPairAsync(
            user,
            roles.ToArray());

        await _dbContext.SaveChangesAsync(cancellationToken);

        return ApiResponse.Ok(
            response,
            "Login completed successfully.");
    }

    public async Task<ApiResponse<AuthResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tokenHash = _tokenService.HashRefreshToken(
            request.RefreshToken);

        await using var transaction =
            await _dbContext.Database.BeginTransactionAsync(
                IsolationLevel.Serializable,
                cancellationToken);

        var storedToken = await _dbContext.RefreshTokens
            .Include(token => token.User)
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is null)
        {
            return InvalidRefreshToken();
        }

        var nowUtc = UtcNow;

        if (storedToken.RevokedAtUtc is not null)
        {
            await RevokeAllActiveTokensAsync(
                storedToken.UserId,
                nowUtc,
                "Refresh token reuse detected.",
                cancellationToken);

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return InvalidRefreshToken();
        }

        if (!storedToken.IsActive(nowUtc))
        {
            return InvalidRefreshToken();
        }

        if (!await IsEmployeeAccountEnabledAsync(
                storedToken.UserId,
                cancellationToken))
        {
            await RevokeAllActiveTokensAsync(
                storedToken.UserId,
                nowUtc,
                "Employee account disabled.",
                cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return InvalidRefreshToken();
        }

        var roles = await _userManager.GetRolesAsync(storedToken.User);
        var permissions = await GetPermissionsAsync(
            storedToken.User,
            roles.ToArray());
        var replacement = _tokenService.CreateRefreshToken();
        var accessToken = _tokenService.CreateAccessToken(
            await ToTokenUserAsync(storedToken.User, cancellationToken),
            roles.ToArray(),
            permissions);

        storedToken.RevokedAtUtc = nowUtc;
        storedToken.RevocationReason = "Rotated.";
        storedToken.ReplacedByTokenHash = replacement.TokenHash;

        AddRefreshToken(storedToken.UserId, replacement);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return InvalidRefreshToken();
        }

        return ApiResponse.Ok(
            ToAuthResponse(accessToken, replacement),
            "Token refreshed successfully.");
    }

    public async Task<ApiResponse<object?>> LogoutAsync(
        LogoutRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var tokenHash = _tokenService.HashRefreshToken(
            request.RefreshToken);
        var storedToken = await _dbContext.RefreshTokens
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        if (storedToken is not null && storedToken.RevokedAtUtc is null)
        {
            storedToken.RevokedAtUtc = UtcNow;
            storedToken.RevocationReason = "Logged out.";

            try
            {
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException)
            {
                // Logout is intentionally idempotent.
            }
        }

        return ApiResponse.Success("Logout completed successfully.");
    }

    public async Task<ApiResponse<CurrentUserResponse>> GetCurrentUserAsync(
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(_currentUser.UserId.ToString())
            ?? throw new HRestaurant.Exceptions.UnauthorizedException(
                "The authenticated user no longer exists.");
        var roles = await _userManager.GetRolesAsync(user);
        var permissions = await GetPermissionsAsync(user, roles.ToArray());
        var branchId = await GetBranchIdAsync(user.Id, cancellationToken);

        return ApiResponse.Ok(
            new CurrentUserResponse(
                user.Id,
                user.FullName,
                user.Email ?? string.Empty,
                user.RestaurantId,
                branchId,
                roles.ToArray(),
                permissions,
                user.EmailConfirmed),
            "Current user retrieved successfully.");
    }

    public async Task<ApiResponse<object?>> RequestPasswordResetAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is not null)
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var url = BuildPublicUrl(
                "/reset-password",
                ("email", user.Email ?? request.Email),
                ("token", token));
            await _accountEmailSender.SendPasswordResetAsync(
                user.Email ?? request.Email,
                user.FullName,
                url,
                cancellationToken);
        }

        return ApiResponse.Success(
            "If the account exists, password reset instructions have been sent.");
    }

    public async Task<ApiResponse<object?>> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return InvalidAccountToken("Password reset token is invalid or expired.");
        }

        var result = await _userManager.ResetPasswordAsync(
            user,
            request.Token,
            request.NewPassword);
        if (!result.Succeeded)
        {
            return IdentityObjectFailure(
                result,
                StatusCodes.Status400BadRequest,
                "Password reset token is invalid or expired.");
        }

        await RevokeAllActiveTokensAsync(
            user.Id,
            UtcNow,
            "Password changed.",
            cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return ApiResponse.Success("Password reset completed successfully.");
    }

    public async Task<ApiResponse<object?>> ResendEmailVerificationAsync(
        ResendVerificationRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is not null && !user.EmailConfirmed)
        {
            await SendVerificationEmailAsync(user, cancellationToken);
        }

        return ApiResponse.Success(
            "If the account requires verification, an email has been sent.");
    }

    public async Task<ApiResponse<object?>> VerifyEmailAsync(
        VerifyEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByIdAsync(request.UserId.ToString());
        if (user is null)
        {
            return InvalidAccountToken("Email verification token is invalid or expired.");
        }

        var result = await _userManager.ConfirmEmailAsync(user, request.Token);
        return result.Succeeded
            ? ApiResponse.Success("Email verified successfully.")
            : IdentityObjectFailure(
                result,
                StatusCodes.Status400BadRequest,
                "Email verification token is invalid or expired.");
    }

    private async Task SendVerificationEmailAsync(
        AppUser user,
        CancellationToken cancellationToken)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var url = BuildPublicUrl(
            "/verify-email",
            ("userId", user.Id.ToString()),
            ("token", token));
        await _accountEmailSender.SendEmailVerificationAsync(
            user.Email ?? string.Empty,
            user.FullName,
            url,
            cancellationToken);
    }

    private string BuildPublicUrl(
        string path,
        params (string Key, string Value)[] values)
    {
        var query = string.Join(
            "&",
            values.Select(value =>
                $"{Uri.EscapeDataString(value.Key)}={Uri.EscapeDataString(value.Value)}"));
        return $"{_publicSettings.PublicBaseUrl.TrimEnd('/')}{path}?{query}";
    }

    private static ApiResponse<object?> InvalidAccountToken(string message) =>
        ApiResponse.Failure<object?>(
            StatusCodes.Status400BadRequest,
            message,
            [new ErrorResponse("invalid_token", message)]);

    private static ApiResponse<object?> IdentityObjectFailure(
        IdentityResult result,
        int statusCode,
        string message) =>
        ApiResponse.Failure<object?>(
            statusCode,
            message,
            result.Errors.Select(error => new ErrorResponse(
                error.Code,
                error.Description)));

    private DateTime UtcNow =>
        _timeProvider.GetUtcNow().UtcDateTime;

    private async Task<IdentityResult> EnsureDefaultRoleAsync()
    {
        if (await _roleManager.RoleExistsAsync(DefaultRole))
        {
            return IdentityResult.Success;
        }

        return await _roleManager.CreateAsync(
            new AppRole
            {
                Id = Guid.NewGuid(),
                Name = DefaultRole,
                CreatedAtUtc = UtcNow
            });
    }

    private async Task<AuthResponse> CreateTokenPairAsync(
        AppUser user,
        IReadOnlyCollection<string> roles)
    {
        var permissions = await GetPermissionsAsync(user, roles);
        var accessToken = _tokenService.CreateAccessToken(
            await ToTokenUserAsync(user, CancellationToken.None),
            roles,
            permissions);
        var refreshToken = _tokenService.CreateRefreshToken();

        AddRefreshToken(user.Id, refreshToken);

        return ToAuthResponse(accessToken, refreshToken);
    }

    private async Task<IReadOnlyCollection<string>> GetPermissionsAsync(
        AppUser user,
        IReadOnlyCollection<string> roles)
    {
        var permissions = new HashSet<string>(
            StringComparer.OrdinalIgnoreCase);
        var userClaims = await _userManager.GetClaimsAsync(user);

        AddPermissionClaims(permissions, userClaims);

        foreach (var roleName in roles)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role is null)
            {
                continue;
            }

            AddPermissionClaims(
                permissions,
                await _roleManager.GetClaimsAsync(role));
        }

        return permissions.ToArray();
    }

    private static void AddPermissionClaims(
        ISet<string> permissions,
        IEnumerable<Claim> claims)
    {
        foreach (var claim in claims.Where(claim =>
                     string.Equals(
                         claim.Type,
                         AuthClaimTypes.Permission,
                         StringComparison.Ordinal)
                     && !string.IsNullOrWhiteSpace(claim.Value)))
        {
            permissions.Add(claim.Value);
        }
    }

    private void AddRefreshToken(
        Guid userId,
        RefreshTokenResult token)
    {
        _dbContext.RefreshTokens.Add(
            new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                TokenHash = token.TokenHash,
                CreatedAtUtc = token.CreatedAtUtc,
                ExpiresAtUtc = token.ExpiresAtUtc
            });
    }

    private async Task RevokeAllActiveTokensAsync(
        Guid userId,
        DateTime nowUtc,
        string reason,
        CancellationToken cancellationToken)
    {
        var activeTokens = await _dbContext.RefreshTokens
            .Where(token =>
                token.UserId == userId
                && token.RevokedAtUtc == null
                && token.ExpiresAtUtc > nowUtc)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            token.RevokedAtUtc = nowUtc;
            token.RevocationReason = reason;
        }
    }

    private async Task<bool> IsEmployeeAccountEnabledAsync(
        Guid appUserId,
        CancellationToken cancellationToken)
    {
        var employeeState = await _dbContext.BusinessUsers
            .AsNoTracking()
            .Where(employee => employee.AppUserId == appUserId)
            .Select(employee => new { employee.IsDeleted, employee.IsActive })
            .SingleOrDefaultAsync(cancellationToken);

        return employeeState is null
            || (!employeeState.IsDeleted && employeeState.IsActive);
    }

    private async Task<TokenUser> ToTokenUserAsync(
        AppUser user,
        CancellationToken cancellationToken)
    {
        return new TokenUser(
            user.Id,
            user.Email!,
            user.RestaurantId,
            user.FullName,
            await GetBranchIdAsync(user.Id, cancellationToken));
    }

    private async Task<Guid?> GetBranchIdAsync(
        Guid appUserId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.BusinessUsers
            .AsNoTracking()
            .Where(employee =>
                employee.AppUserId == appUserId
                && !employee.IsDeleted
                && employee.IsActive)
            .Select(employee => employee.BranchId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    private static AuthResponse ToAuthResponse(
        AccessTokenResult accessToken,
        RefreshTokenResult refreshToken)
    {
        return new AuthResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshToken.Token,
            refreshToken.ExpiresAtUtc);
    }

    private static ApiResponse<AuthResponse> InvalidCredentials()
    {
        return ApiResponse.Failure<AuthResponse>(
            StatusCodes.Status401Unauthorized,
            InvalidCredentialsMessage,
            [
                new ErrorResponse(
                    "invalid_credentials",
                    InvalidCredentialsMessage)
            ]);
    }

    private static ApiResponse<AuthResponse> InvalidRefreshToken()
    {
        return ApiResponse.Failure<AuthResponse>(
            StatusCodes.Status401Unauthorized,
            InvalidRefreshTokenMessage,
            [
                new ErrorResponse(
                    "invalid_refresh_token",
                    InvalidRefreshTokenMessage)
            ]);
    }

    private static ApiResponse<AuthResponse> IdentityFailure(
        IdentityResult result,
        int statusCode,
        string message)
    {
        return ApiResponse.Failure<AuthResponse>(
            statusCode,
            message,
            result.Errors.Select(error => new ErrorResponse(
                error.Code,
                error.Description)));
    }
}
