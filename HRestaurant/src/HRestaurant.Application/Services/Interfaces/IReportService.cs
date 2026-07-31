using HRestaurant.DTOS.Reports;
using HRestaurant.DTOS.Responses;

namespace HRestaurant.Services.Interfaces;

public interface IReportService
{
    Task<ApiResponse<DashboardSummaryDTO>> GetDashboardAsync(ReportQuery query,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<TimeSeriesPointDTO>>> GetSalesAsync(
        ReportQuery query, string period, CancellationToken cancellationToken = default);
    Task<PagedResponse<NamedValueDTO>> GetMenuItemSalesAsync(ReportQuery query, bool least,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<NamedValueDTO>>> GetSalesByCategoryAsync(ReportQuery query,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<NamedValueDTO>>> GetSalesByPaymentMethodAsync(ReportQuery query,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<NamedValueDTO>>> GetSalesByBranchAsync(ReportQuery query,
        CancellationToken cancellationToken = default);
    Task<PagedResponse<EmployeePerformanceDTO>> GetEmployeePerformanceAsync(ReportQuery query,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<NamedValueDTO>>> GetPeakHoursAsync(ReportQuery query,
        CancellationToken cancellationToken = default);
    Task<PagedResponse<RecentOrderDTO>> GetCancelledOrdersAsync(ReportQuery query,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<ReservationStatisticsDTO>> GetReservationStatisticsAsync(ReportQuery query,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<CustomerStatisticsDTO>> GetCustomerStatisticsAsync(ReportQuery query,
        CancellationToken cancellationToken = default);
    Task<ApiResponse<IReadOnlyCollection<NamedValueDTO>>> GetLowStockSummaryAsync(ReportQuery query,
        CancellationToken cancellationToken = default);
}
