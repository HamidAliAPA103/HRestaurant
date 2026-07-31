using HRestaurant.DTOS.Responses;

namespace HRestaurant.DTOS.Customer;

public sealed class CustomerCreateDTO
{
    public Guid RestaurantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateOnly? Birthday { get; set; }
    public string? Notes { get; set; }
}

public sealed class CustomerUpdateDTO
{
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateOnly? Birthday { get; set; }
    public string? Notes { get; set; }
}

public sealed class CustomerListRequest
{
    public int PageNumber { get; set; } = PaginationRequest.DefaultPageNumber;
    public int PageSize { get; set; } = PaginationRequest.DefaultPageSize;
    public string? Search { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
}

public class CustomerGetDTO
{
    public Guid Id { get; set; }
    public Guid RestaurantId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateOnly? Birthday { get; set; }
    public string? Notes { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public DateTime? LastVisitDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class CustomerOrderHistoryDTO
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class CustomerReservationHistoryDTO
{
    public Guid ReservationId { get; set; }
    public string BranchName { get; set; } = string.Empty;
    public string TableNumber { get; set; } = string.Empty;
    public DateTime ReservationTime { get; set; }
    public int GuestCount { get; set; }
    public string Status { get; set; } = string.Empty;
}

public sealed class FavoriteMenuItemDTO
{
    public Guid MenuItemId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal TotalSpent { get; set; }
}

public sealed class CustomerDetailDTO : CustomerGetDTO
{
    public List<FavoriteMenuItemDTO> FavoriteMenuItems { get; set; } = [];
}
