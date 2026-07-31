using HRestaurant.DTOS.Branch;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IBranchService
{
    Task<ApiResponse<Guid>> CreateAsync(
        BranchCreateDTO dto,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<BranchGetDTO>> GetAllAsync(
        BranchListRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedResponse<BranchGetDTO>> GetByRestaurantAsync(
        Guid restaurantId,
        BranchListRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<BranchGetDTO>> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> UpdateAsync(
        Guid id,
        BranchUpdateDTO dto,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> SoftDeleteAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> ActivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> DeactivateAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> AssignManagerAsync(
        Guid id,
        BranchManagerAssignmentDTO dto,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> RemoveManagerAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiResponse<IReadOnlyCollection<BranchWorkingHourDTO>>>
        GetWorkingHoursAsync(
            Guid id,
            CancellationToken cancellationToken = default);

    Task<ApiResponse<object?>> UpdateWorkingHoursAsync(
        Guid id,
        BranchWorkingHoursUpdateDTO dto,
        CancellationToken cancellationToken = default);
}
