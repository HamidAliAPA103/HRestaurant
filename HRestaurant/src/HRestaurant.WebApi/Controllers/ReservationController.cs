using HRestaurant.DTOS.Reservation;
using HRestaurant.DTOS.Responses;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + ","
    + AppRoles.Manager + "," + AppRoles.Waiter)]
[Route("api/reservations")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public sealed class ReservationController : ApiControllerBase
{
    private readonly IReservationService _service;
    public ReservationController(IReservationService service) => _service = service;

    [HttpPost, PermissionAuthorize(Permissions.Reservations.Manage)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(ReservationCreateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.CreateAsync(dto, cancellationToken));

    [HttpGet, PermissionAuthorize(Permissions.Reservations.Read)]
    [ProducesResponseType(typeof(PagedResponse<ReservationGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] ReservationListRequest request,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetAllAsync(request, cancellationToken));

    [HttpGet("{id:guid}"), PermissionAuthorize(Permissions.Reservations.Read)]
    [ProducesResponseType(typeof(ApiResponse<ReservationGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}"), PermissionAuthorize(Permissions.Reservations.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, ReservationUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateAsync(id, dto, cancellationToken));

    [HttpPatch("{id:guid}/status"), PermissionAuthorize(Permissions.Reservations.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateStatus(Guid id,
        ReservationStatusUpdateDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateStatusAsync(id, dto, cancellationToken));

    [HttpDelete("{id:guid}"), PermissionAuthorize(Permissions.Reservations.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.RemoveAsync(id, cancellationToken));
}
