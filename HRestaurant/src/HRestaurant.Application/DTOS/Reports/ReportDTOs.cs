using HRestaurant.DTOS.Responses;

namespace HRestaurant.DTOS.Reports;

public sealed class ReportQuery
{
    public DateOnly? From { get; set; }
    public DateOnly? To { get; set; }
    public Guid? BranchId { get; set; }
    public int PageNumber { get; set; } = PaginationRequest.DefaultPageNumber;
    public int PageSize { get; set; } = 20;
}

public sealed class DashboardSummaryDTO
{
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public int ReservationCount { get; set; }
    public int CustomerCount { get; set; }
    public int LowStockCount { get; set; }
    public decimal RefundedAmount { get; set; }
    public List<RecentOrderDTO> RecentOrders { get; set; } = [];
    public List<NamedValueDTO> TopItems { get; set; } = [];
    public List<TimeSeriesPointDTO> Sales { get; set; } = [];
}

public sealed class RecentOrderDTO
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public sealed class TimeSeriesPointDTO
{
    public DateTime Period { get; set; }
    public decimal Revenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public sealed class NamedValueDTO
{
    public string Name { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public int Count { get; set; }
}

public sealed class EmployeePerformanceDTO
{
    public Guid EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int OrderCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public sealed class ReservationStatisticsDTO
{
    public int Total { get; set; }
    public int Pending { get; set; }
    public int Confirmed { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int NoShow { get; set; }
    public decimal CancellationRate { get; set; }
}

public sealed class CustomerStatisticsDTO
{
    public int TotalCustomers { get; set; }
    public int ReturningCustomers { get; set; }
    public int NewCustomers { get; set; }
    public decimal AverageCustomerSpend { get; set; }
}
