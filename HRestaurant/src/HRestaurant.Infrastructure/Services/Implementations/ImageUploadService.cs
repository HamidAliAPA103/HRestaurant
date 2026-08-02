using HRestaurant.DTOS.Common;
using HRestaurant.DTOS.Responses;
using HRestaurant.Exceptions;
using HRestaurant.Extentions;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace HRestaurant.Services.Implementations;

public sealed class ImageUploadService : IImageUploadService
{
    private static readonly HashSet<string> Categories =
        ["restaurant-logo", "restaurant-cover", "employee-avatar"];
    private readonly IWebHostEnvironment _environment;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICurrentUserContext _currentUser;

    public ImageUploadService(
        IWebHostEnvironment environment,
        IHttpContextAccessor httpContextAccessor,
        ICurrentUserContext currentUser)
    {
        _environment = environment;
        _httpContextAccessor = httpContextAccessor;
        _currentUser = currentUser;
    }

    public async Task<ApiResponse<ImageUploadResponse>> UploadAsync(
        FileUploadDTO file,
        string category,
        Guid? restaurantId,
        string? oldImageUrl,
        CancellationToken cancellationToken = default)
    {
        var scopeId = ResolveRestaurantId(restaurantId);
        var normalizedCategory = NormalizeCategory(category);
        var webRoot = _environment.WebRootPath
            ?? throw new InvalidOperationException("The web root is not configured.");
        var fileName = await file.CreateFileAsync(
            cancellationToken,
            webRoot,
            "images",
            "uploads",
            scopeId.ToString("N"),
            normalizedCategory);

        if (!string.IsNullOrWhiteSpace(oldImageUrl))
            DeleteLocalFile(oldImageUrl, scopeId, webRoot);

        var request = _httpContextAccessor.HttpContext?.Request
            ?? throw new InvalidOperationException("The current HTTP request is unavailable.");
        var relativeUrl = $"/images/uploads/{scopeId:N}/{normalizedCategory}/{fileName}";
        var publicUrl = $"{request.Scheme}://{request.Host}{request.PathBase}{relativeUrl}";
        return ApiResponse.Created(
            new ImageUploadResponse(publicUrl, fileName, file.Length),
            "Image uploaded successfully.");
    }

    public Task<ApiResponse<object?>> DeleteAsync(
        string imageUrl,
        Guid? restaurantId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var scopeId = ResolveRestaurantId(restaurantId);
        var webRoot = _environment.WebRootPath
            ?? throw new InvalidOperationException("The web root is not configured.");
        DeleteLocalFile(imageUrl, scopeId, webRoot);
        return Task.FromResult(ApiResponse.NoContent("Image deleted successfully."));
    }

    private Guid ResolveRestaurantId(Guid? restaurantId)
    {
        if (!_currentUser.IsSuperAdmin)
        {
            if (restaurantId.HasValue && restaurantId.Value != _currentUser.RestaurantId)
                throw new ForbiddenException("Images can be managed only for the current restaurant.");
            return _currentUser.RestaurantId;
        }
        return restaurantId ?? _currentUser.RestaurantId;
    }

    private static string NormalizeCategory(string category)
    {
        var normalized = category.Trim().ToLowerInvariant();
        if (!Categories.Contains(normalized))
            throw new ValidationException(new Dictionary<string, string[]>
            {
                ["Category"] = ["Image category is invalid."]
            });
        return normalized;
    }

    private static void DeleteLocalFile(string imageUrl, Guid restaurantId, string webRoot)
    {
        if (!Uri.TryCreate(imageUrl, UriKind.RelativeOrAbsolute, out var uri)) return;
        var path = uri.IsAbsoluteUri ? uri.AbsolutePath : uri.OriginalString.Split('?', '#')[0];
        var expectedPrefix = $"/images/uploads/{restaurantId:N}/";
        if (!path.StartsWith(expectedPrefix, StringComparison.OrdinalIgnoreCase)) return;
        var relative = path.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var root = Path.GetFullPath(webRoot).TrimEnd(Path.DirectorySeparatorChar);
        var target = Path.GetFullPath(Path.Combine(root, relative));
        if (target.StartsWith(root + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase)
            && File.Exists(target))
            File.Delete(target);
    }
}
