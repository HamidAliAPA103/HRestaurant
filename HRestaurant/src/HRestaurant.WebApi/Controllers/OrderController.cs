using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.OrderItem;
using HRestaurant.DTOS.Responses;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + ","
    + AppRoles.Manager + "," + AppRoles.Cashier + "," + AppRoles.Waiter + "," + AppRoles.Chef)]
[PermissionAuthorize(Permissions.Orders.Read)]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
[Route("api/orders")]
[Route("api/Order")]
public sealed class OrderController : ApiControllerBase
{
    private const string SalesRoles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + ","
        + AppRoles.Manager + "," + AppRoles.Cashier + "," + AppRoles.Waiter;
    private const string KitchenRoles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + ","
        + AppRoles.Manager + "," + AppRoles.Chef;
    private readonly IOrderService _service;

    public OrderController(IOrderService service) => _service = service;

    [HttpPost, Authorize(Roles = SalesRoles), PermissionAuthorize(Permissions.Orders.Create)]
    [SwaggerOperation(Summary = "Create an order using server-side menu prices")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(OrderCreatDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.CreateAsync(dto, cancellationToken));

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<OrderGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] OrderListRequest request,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetAllAsync(request, cancellationToken));

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<OrderGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByIdAsync(id, cancellationToken));

    [HttpGet("branch/{branchId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<OrderGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByBranch(Guid branchId, [FromQuery] OrderListRequest request,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByBranchAsync(branchId, request, cancellationToken));

    [HttpGet("waiter/{waiterId:guid}")]
    [ProducesResponseType(typeof(PagedResponse<OrderGetDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByWaiter(Guid waiterId, [FromQuery] OrderListRequest request,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetByWaiterAsync(waiterId, request, cancellationToken));

    [HttpPut("{id:guid}"), Authorize(Roles = SalesRoles), PermissionAuthorize(Permissions.Orders.Update)]
    public async Task<IActionResult> Update(Guid id, OrderUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateAsync(id, dto, cancellationToken));

    [HttpPost("{id:guid}/items"), Authorize(Roles = SalesRoles), PermissionAuthorize(Permissions.Orders.Update)]
    public async Task<IActionResult> AddItem(Guid id, OrderItemAddDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.AddItemAsync(id, dto, cancellationToken));

    [HttpPut("{id:guid}/items/{itemId:guid}/quantity"), Authorize(Roles = SalesRoles),
     PermissionAuthorize(Permissions.Orders.Update)]
    public async Task<IActionResult> UpdateItemQuantity(Guid id, Guid itemId, OrderItemUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateItemQuantityAsync(id, itemId, dto, cancellationToken));

    [HttpPut("{id:guid}/items/{itemId:guid}/kitchen-note"), Authorize(Roles = SalesRoles),
     PermissionAuthorize(Permissions.Orders.Update)]
    public async Task<IActionResult> UpdateKitchenNote(Guid id, Guid itemId,
        OrderItemKitchenNoteDTO dto, CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateItemKitchenNoteAsync(id, itemId, dto, cancellationToken));

    [HttpDelete("{id:guid}/items/{itemId:guid}"), Authorize(Roles = SalesRoles),
     PermissionAuthorize(Permissions.Orders.Update)]
    public async Task<IActionResult> RemoveItem(Guid id, Guid itemId, OrderConcurrencyDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.RemoveItemAsync(id, itemId, dto.RowVersion, cancellationToken));

    [HttpPatch("{id:guid}/status"), Authorize(Roles = SalesRoles),
     PermissionAuthorize(Permissions.Orders.Update)]
    public async Task<IActionResult> UpdateStatus(Guid id, OrderStatusUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateStatusAsync(id, dto, cancellationToken));

    [HttpPatch("{id:guid}/kitchen-status"), Authorize(Roles = KitchenRoles),
     PermissionAuthorize(Permissions.Orders.UpdateKitchenStatus)]
    public async Task<IActionResult> UpdateKitchenStatus(Guid id, KitchenOrderStatusUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.UpdateKitchenStatusAsync(id, dto, cancellationToken));

    [HttpPost("{id:guid}/cancel"), Authorize(Roles = SalesRoles),
     PermissionAuthorize(Permissions.Orders.Update)]
    public async Task<IActionResult> Cancel(Guid id, OrderCancelDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.CancelAsync(id, dto, cancellationToken));

    [HttpPatch("{id:guid}/table"), Authorize(Roles = SalesRoles),
     PermissionAuthorize(Permissions.Orders.Update)]
    public async Task<IActionResult> ChangeTable(Guid id, OrderTableUpdateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.ChangeTableAsync(id, dto, cancellationToken));

    [HttpPost("{id:guid}/discount"), Authorize(Roles = SalesRoles),
     PermissionAuthorize(Permissions.Orders.Update)]
    public async Task<IActionResult> ApplyDiscount(Guid id, OrderDiscountDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.ApplyDiscountAsync(id, dto, cancellationToken));

    [HttpPost("{id:guid}/merge"), Authorize(Roles = SalesRoles),
     PermissionAuthorize(Permissions.Orders.Update)]
    public async Task<IActionResult> Merge(Guid id, OrderMergeDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.MergeAsync(id, dto, cancellationToken));

    [HttpPost("{id:guid}/split"), Authorize(Roles = SalesRoles),
     PermissionAuthorize(Permissions.Orders.Update)]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Split(Guid id, OrderSplitDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.SplitAsync(id, dto, cancellationToken));

    [HttpGet("kitchen"), Authorize(Roles = KitchenRoles),
     PermissionAuthorize(Permissions.Orders.Read)]
    [ProducesResponseType(typeof(ApiResponse<KitchenDashboardDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetKitchenDashboard([FromQuery] Guid? branchId,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetKitchenDashboardAsync(branchId, cancellationToken));

    [HttpPost("{id:guid}/payment"), Authorize(Policy = AuthorizationPolicies.PaymentProcessing),
     PermissionAuthorize(Permissions.Payments.Process)]
    public async Task<IActionResult> ProcessPayment(Guid id, OrderConcurrencyDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.ProcessPaymentAsync(id, dto.RowVersion, cancellationToken));
}
