using HRestaurant.DTOS.Responses;
using HRestaurant.DTOS.Shift;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + "," + AppRoles.Manager)]
[PermissionAuthorize(Permissions.Shifts.Read)]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
[Route("api/Shift")]
public sealed class ShiftController : ApiControllerBase
{
    private readonly IShiftService _service;

    public ShiftController(IShiftService service) => _service = service;

    [HttpPost]
    [PermissionAuthorize(Permissions.Shifts.Manage)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(ShiftCreateDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.CreateAsync(dto, cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<ShiftGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShifts(
        [FromQuery] ShiftListRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetShiftsAsync(request, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<ShiftGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}")]
    [PermissionAuthorize(Permissions.Shifts.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(
        Guid id, ShiftUpdateDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateAsync(id, dto, cancellationToken));

    [HttpDelete("{id:guid}")]
    [PermissionAuthorize(Permissions.Shifts.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> SoftDelete(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.SoftDeleteAsync(id, cancellationToken));

    [HttpPost("assignments")]
    [PermissionAuthorize(Permissions.Shifts.Manage)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignEmployee(
        EmployeeShiftAssignDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.AssignEmployeeAsync(dto, cancellationToken));

    [HttpDelete("assignments/{assignmentId:guid}")]
    [PermissionAuthorize(Permissions.Shifts.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveEmployee(
        Guid assignmentId, CancellationToken cancellationToken) =>
        FromResponse(await _service.RemoveEmployeeAsync(assignmentId, cancellationToken));

    [HttpGet("assignments")]
    [ProducesResponseType(typeof(PagedResponse<EmployeeShiftGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignments(
        [FromQuery] ShiftListRequest request, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetAssignmentsAsync(request, cancellationToken));

    [HttpGet("assignments/daily/{date}")]
    [ProducesResponseType(typeof(PagedResponse<EmployeeShiftGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDaily(
        DateOnly date, [FromQuery] ShiftListRequest request, CancellationToken cancellationToken)
    {
        request.FromDate = date;
        request.ToDate = date;
        return FromResponse(await _service.GetAssignmentsAsync(request, cancellationToken));
    }

    [HttpGet("assignments/weekly/{weekStart}")]
    [ProducesResponseType(typeof(PagedResponse<EmployeeShiftGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWeekly(
        DateOnly weekStart, [FromQuery] ShiftListRequest request, CancellationToken cancellationToken)
    {
        request.FromDate = weekStart;
        request.ToDate = weekStart.AddDays(6);
        return FromResponse(await _service.GetAssignmentsAsync(request, cancellationToken));
    }

    [HttpGet("assignments/employee/{employeeId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<EmployeeShiftGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByEmployee(
        Guid employeeId, [FromQuery] ShiftListRequest request, CancellationToken cancellationToken)
    {
        request.EmployeeId = employeeId;
        return FromResponse(await _service.GetAssignmentsAsync(request, cancellationToken));
    }

    [HttpGet("assignments/branch/{branchId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<EmployeeShiftGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByBranch(
        Guid branchId, [FromQuery] ShiftListRequest request, CancellationToken cancellationToken)
    {
        request.BranchId = branchId;
        return FromResponse(await _service.GetAssignmentsAsync(request, cancellationToken));
    }
}
