using HRestaurant.DTOS.Customer;
using HRestaurant.DTOS.Responses;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + ","
    + AppRoles.Manager + "," + AppRoles.Cashier + "," + AppRoles.Waiter)]
[PermissionAuthorize(Permissions.Customers.Read)]
[Route("api/customers")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public sealed class CustomerController : ApiControllerBase
{
    private const string ManageRoles = AppRoles.SuperAdmin + ","
        + AppRoles.RestaurantOwner + "," + AppRoles.Manager + ","
        + AppRoles.Cashier + "," + AppRoles.Waiter;
    private readonly ICustomerService _service;

    public CustomerController(ICustomerService service) => _service = service;

    [HttpPost, Authorize(Roles = ManageRoles),
     PermissionAuthorize(Permissions.Customers.Manage)]
    [SwaggerOperation(Summary = "Create a restaurant customer")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(CustomerCreateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.CreateAsync(dto, cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<CustomerGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] CustomerListRequest request,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetAllAsync(request, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<CustomerDetailDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByIdAsync(id, cancellationToken));

    [HttpPut("{id:guid}"), Authorize(Roles = ManageRoles),
     PermissionAuthorize(Permissions.Customers.Manage)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Update(Guid id, CustomerUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateAsync(id, dto, cancellationToken));

    [HttpDelete("{id:guid}"), Authorize(Roles = ManageRoles),
     PermissionAuthorize(Permissions.Customers.Manage)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.SoftDeleteAsync(id, cancellationToken));

    [HttpGet("{id:guid}/orders")]
    [ProducesResponseType(typeof(PagedResponse<CustomerOrderHistoryDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrders(Guid id, [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default) =>
        FromResponse(await _service.GetOrderHistoryAsync(
            id, pageNumber, Math.Clamp(pageSize, 1, PaginationRequest.MaxPageSize),
            cancellationToken));

    [HttpGet("{id:guid}/reservations")]
    [ProducesResponseType(typeof(PagedResponse<CustomerReservationHistoryDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReservations(Guid id,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        FromResponse(await _service.GetReservationHistoryAsync(
            id, pageNumber, Math.Clamp(pageSize, 1, PaginationRequest.MaxPageSize),
            cancellationToken));

    [HttpGet("{id:guid}/favorites")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<FavoriteMenuItemDTO>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFavorites(Guid id,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetFavoritesAsync(id, cancellationToken));
}
