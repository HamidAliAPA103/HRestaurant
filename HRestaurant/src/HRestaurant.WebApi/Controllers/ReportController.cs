using HRestaurant.DTOS.Reports;
using HRestaurant.DTOS.Responses;
using HRestaurant.Infrastructure.Authentication;
using HRestaurant.Infrastructure.Authorization;
using HRestaurant.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HRestaurant.Controllers;

[Authorize(Roles = AppRoles.SuperAdmin + "," + AppRoles.RestaurantOwner + ","
    + AppRoles.Manager)]
[PermissionAuthorize(Permissions.Reports.Read)]
[Route("api/reports")]
[Produces("application/json")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public sealed class ReportController : ApiControllerBase
{
    private readonly IReportService _service;
    public ReportController(IReportService service) => _service = service;

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardSummaryDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Dashboard([FromQuery] ReportQuery query,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetDashboardAsync(query, cancellationToken));

    [HttpGet("sales")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<TimeSeriesPointDTO>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Sales([FromQuery] ReportQuery query,
        [FromQuery] string period = "day", CancellationToken cancellationToken = default) =>
        FromResponse(await _service.GetSalesAsync(query, period, cancellationToken));

    [HttpGet("menu-items")]
    [ProducesResponseType(typeof(PagedResponse<NamedValueDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> MenuItems([FromQuery] ReportQuery query,
        [FromQuery] bool least = false, CancellationToken cancellationToken = default) =>
        FromResponse(await _service.GetMenuItemSalesAsync(query, least, cancellationToken));

    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<NamedValueDTO>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Categories([FromQuery] ReportQuery query,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetSalesByCategoryAsync(query, cancellationToken));

    [HttpGet("payment-methods")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<NamedValueDTO>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> PaymentMethods([FromQuery] ReportQuery query,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetSalesByPaymentMethodAsync(query, cancellationToken));

    [HttpGet("branches")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<NamedValueDTO>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Branches([FromQuery] ReportQuery query,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetSalesByBranchAsync(query, cancellationToken));

    [HttpGet("employees")]
    [ProducesResponseType(typeof(PagedResponse<EmployeePerformanceDTO>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Employees([FromQuery] ReportQuery query,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetEmployeePerformanceAsync(query, cancellationToken));

    [HttpGet("peak-hours")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<NamedValueDTO>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> PeakHours([FromQuery] ReportQuery query,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetPeakHoursAsync(query, cancellationToken));

    [HttpGet("cancelled-orders")]
    [ProducesResponseType(typeof(PagedResponse<RecentOrderDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CancelledOrders([FromQuery] ReportQuery query,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetCancelledOrdersAsync(query, cancellationToken));

    [HttpGet("reservations")]
    [ProducesResponseType(typeof(ApiResponse<ReservationStatisticsDTO>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Reservations([FromQuery] ReportQuery query,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetReservationStatisticsAsync(query, cancellationToken));

    [HttpGet("customers")]
    [ProducesResponseType(typeof(ApiResponse<CustomerStatisticsDTO>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> Customers([FromQuery] ReportQuery query,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetCustomerStatisticsAsync(query, cancellationToken));

    [HttpGet("low-stock")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyCollection<NamedValueDTO>>),
        StatusCodes.Status200OK)]
    public async Task<IActionResult> LowStock([FromQuery] ReportQuery query,
        CancellationToken cancellationToken) =>
        FromResponse(await _service.GetLowStockSummaryAsync(query, cancellationToken));
}
