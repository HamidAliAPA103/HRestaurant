using HRestaurant.DTOS.Order;
using HRestaurant.DTOS.Responses;
using HRestaurant.Enum;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Route("api/[controller]")]
public sealed class OrderController : ApiControllerBase
{
    private readonly IOrderService _service;

    public OrderController(IOrderService service)
    {
        ArgumentNullException.ThrowIfNull(service);
        _service = service;
    }

    [HttpPost]
    [PermissionAuthorize(Permissions.Orders.Create)]
    public async Task<IActionResult> Create(
        OrderCreatDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.CreateAsync(dto, cancellationToken));
    }

    [HttpDelete]
    [PermissionAuthorize(Permissions.Orders.Delete)]
    public async Task<IActionResult> Remove(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.RemoveAsync(id, cancellationToken));
    }

    [HttpGet]
    [PermissionAuthorize(Permissions.Orders.Read)]
    public async Task<IActionResult> GetAll(
        ViewType type,
        [FromQuery] PaginationRequest pagination,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.GetAllAsync(
                type,
                pagination,
                cancellationToken));
    }

    [HttpPatch]
    [PermissionAuthorize(Permissions.Orders.Update)]
    public async Task<IActionResult> Update(
        Guid id,
        OrderUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.UpdateAsync(id, dto, cancellationToken));
    }

    [HttpPatch("toggle/{id:guid}")]
    [PermissionAuthorize(Permissions.Orders.Delete)]
    public async Task<IActionResult> Toggle(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.ToggleAsync(id, cancellationToken));
    }

    [HttpGet("{id:guid}")]
    [PermissionAuthorize(Permissions.Orders.Read)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.GetByIdAsync(id, cancellationToken));
    }

    [HttpPatch("{id:guid}/kitchen-status")]
    [PermissionAuthorize(Permissions.Orders.UpdateKitchenStatus)]
    public async Task<IActionResult> UpdateKitchenStatus(
        Guid id,
        KitchenOrderStatusUpdateDTO dto,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.UpdateKitchenStatusAsync(
                id,
                dto.Status,
                cancellationToken));
    }

    [HttpPost("{id:guid}/payment")]
    [Authorize(Policy = AuthorizationPolicies.PaymentProcessing)]
    [PermissionAuthorize(Permissions.Payments.Process)]
    public async Task<IActionResult> ProcessPayment(
        Guid id,
        CancellationToken cancellationToken)
    {
        return FromResponse(
            await _service.ProcessPaymentAsync(
                id,
                cancellationToken));
    }
}
