namespace SalesManagementSystem.Models;

public class OrderDetail
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int ProductId { get; set; }

    public int Quantity { get; set; }

    // NAVIGATION
    public Order Order { get; set; } = null!;

    public Product Product { get; set; } = null!;
}