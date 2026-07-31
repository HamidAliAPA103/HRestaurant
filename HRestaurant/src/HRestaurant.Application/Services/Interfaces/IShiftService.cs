using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Shift;

namespace HRestaurant.Services.Interfaces;

public interface IShiftService
{
    Task<ApiResponse<Guid>> CreateAsync(ShiftCreateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<ShiftGetDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<PagedResponse<ShiftGetDTO>> GetShiftsAsync(ShiftListRequest request, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> UpdateAsync(Guid id, ShiftUpdateDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ApiResponse<Guid>> AssignEmployeeAsync(EmployeeShiftAssignDTO dto, CancellationToken cancellationToken = default);
    Task<ApiResponse<object?>> RemoveEmployeeAsync(Guid assignmentId, CancellationToken cancellationToken = default);
    Task<PagedResponse<EmployeeShiftGetDTO>> GetAssignmentsAsync(ShiftListRequest request, CancellationToken cancellationToken = default);
}
