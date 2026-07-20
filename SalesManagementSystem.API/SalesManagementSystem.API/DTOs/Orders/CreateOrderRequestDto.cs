namespace SalesManagementSystem.API.DTOs.Orders;

public class CreateOrderRequestDto
{
    public List<OrderItemDto> Products { get; set; } = new();
}