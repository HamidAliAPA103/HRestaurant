using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.OrderItem;
using HRestaurant.DTOS.Responses;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + ","
    + AppRoles.Manager + "," + AppRoles.Cashier + "," + AppRoles.Waiter)]
[PermissionAuthorize(Permissions.Orders.Update)]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
[Route("api/order-items")]
public sealed class OrderItemController : ApiControllerBase
{
    private readonly IOrderItemService _service;
    public OrderItemController(IOrderItemService service) => _service = service;

    [HttpPost("orders/{orderId:guid}")]
    public async Task<IActionResult> Add(Guid orderId, OrderItemAddDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.AddAsync(orderId, dto, cancellationToken));

    [HttpPut("orders/{orderId:guid}/{itemId:guid}/quantity")]
    public async Task<IActionResult> UpdateQuantity(Guid orderId, Guid itemId,
        OrderItemUpdateDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateQuantityAsync(orderId, itemId, dto, cancellationToken));

    [HttpPut("orders/{orderId:guid}/{itemId:guid}/kitchen-note")]
    public async Task<IActionResult> UpdateKitchenNote(Guid orderId, Guid itemId,
        OrderItemKitchenNoteDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateKitchenNoteAsync(orderId, itemId, dto, cancellationToken));

    [HttpDelete("orders/{orderId:guid}/{itemId:guid}")]
    public async Task<IActionResult> Remove(Guid orderId, Guid itemId, OrderConcurrencyDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.RemoveAsync(orderId, itemId, dto.RowVersion, cancellationToken));
}
