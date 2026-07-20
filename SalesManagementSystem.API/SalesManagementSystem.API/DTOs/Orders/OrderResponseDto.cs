using SalesManagementSystem.Models.Enums;

namespace SalesManagementSystem.API.DTOs.Orders;

public class OrderResponseDto
{
    public int Id { get; set; }
    public string CustomerName { get; set; } = null!;
    public DateTime OrderDate { get; set; }

    public OrderStatus Status { get; set; }

    public decimal TotalPrice { get; set; }

    public List<OrderDetailResponseDto> Items { get; set; } = new();
}