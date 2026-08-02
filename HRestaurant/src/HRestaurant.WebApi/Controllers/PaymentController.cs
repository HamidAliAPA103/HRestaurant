using HRestaurant.DTOS.Payment;
using HRestaurant.DTOS.Responses;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + ","
    + AppRoles.Manager + "," + AppRoles.Cashier)]
[Route("api/payments")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status409Conflict)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public sealed class PaymentController : ApiControllerBase
{
    private readonly IPaymentService _service;
    public PaymentController(IPaymentService service) => _service = service;

    [HttpPost, PermissionAuthorize(Permissions.Payments.Process)]
    [SwaggerOperation(Summary = "Create a pending payment")]
    [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create(PaymentCreateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.CreateAsync(dto, cancellationToken));

    [HttpPost("{id:guid}/complete"), PermissionAuthorize(Permissions.Payments.Process)]
    [ProducesResponseType(typeof(ApiResponse<OrderPaymentSummaryDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Complete(Guid id, PaymentCompleteDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.CompleteAsync(id, dto, cancellationToken));

    [HttpPost("{id:guid}/fail"), PermissionAuthorize(Permissions.Payments.Process)]
    [ProducesResponseType(typeof(ApiResponse<OrderPaymentSummaryDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Fail(Guid id, PaymentFailedDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.FailAsync(id, dto, cancellationToken));

    [HttpPost("split"), PermissionAuthorize(Permissions.Payments.Process)]
    [ProducesResponseType(typeof(ApiResponse<OrderPaymentSummaryDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Split(SplitPaymentDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.SplitAsync(dto, cancellationToken));

    [HttpGet("orders/{orderId:guid}"), PermissionAuthorize(Permissions.Payments.Read)]
    [ProducesResponseType(typeof(ApiResponse<OrderPaymentSummaryDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrderSummary(Guid orderId,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetOrderSummaryAsync(orderId, cancellationToken));

    [HttpGet("orders/{orderId:guid}/receipt"), PermissionAuthorize(Permissions.Payments.Read)]
    [ProducesResponseType(typeof(ApiResponse<ReceiptDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetReceipt(Guid orderId,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetReceiptAsync(orderId, cancellationToken));

    [HttpPost("{id:guid}/refund"),
     Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + ","
        + AppRoles.Manager), PermissionAuthorize(Permissions.Payments.Refund)]
    [ProducesResponseType(typeof(ApiResponse<OrderPaymentSummaryDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Refund(Guid id, RefundCreateDTO dto,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.RefundAsync(id, dto, cancellationToken));

    [HttpGet("{id:guid}/refunds"), PermissionAuthorize(Permissions.Payments.Read)]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<RefundGetDTO>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRefunds(Guid id,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetRefundHistoryAsync(id, cancellationToken));
}
