using SalesManagementSystem.Models.Enums;

namespace SalesManagementSystem.API.DTOs.Orders;

public class OrderQueryParameters
{
    public OrderStatus? Status { get; set; }

    public int? CustomerId { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int PageNumber { get; set; } = 1;

    public int PageSize { get; set; } = 10;
}