using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.User;

namespace HRestaurant.Services.Interfaces;

public interface IUserService
{
    Task<ApiResponse<Guid>> CreateAsync(UserCreateDTO dto, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserGetDTO>> GetAllAsync(EmployeeListRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserGetDTO>> GetByRestaurantAsync(Guid restaurantId, EmployeeListRequest request, CancellationToken cancellationToken = default);
    Task<PagedResponse<UserGetDTO>> GetByBranchAsync(Guid branchId, EmployeeListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<UserGetDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateAsync(Guid id, UserUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> ActivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> AssignBranchAsync(Guid id, EmployeeBranchAssignmentDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> AssignRoleAsync(Guid id, EmployeeRoleAssignmentDTO dto, CancellationToken cancellationToken = default);
}
