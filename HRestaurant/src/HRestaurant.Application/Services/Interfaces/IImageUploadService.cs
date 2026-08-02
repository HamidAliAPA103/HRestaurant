using HRestaurant.DTOS.Common;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IImageUploadService
{
    Task<ApiResponse<ImageUploadResponse>> UploadAsync(
        FileUploadDTO file,
        string category,
        Guid? restaurantId,
        string? oldImageUrl,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> DeleteAsync(
        string imageUrl,
        Guid? restaurantId,
        CancellationToken cancellationToken = default);
}
