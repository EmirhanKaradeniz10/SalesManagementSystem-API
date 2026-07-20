using System.ComponentModel.DataAnnotations.Schema;
using SalesManagementSystem.Models.Enums;

namespace SalesManagementSystem.Models;


public class Order
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public DateTime OrderDate { get; set; }

    public bool IsCancelled { get; set; } = false;

    //order durumu
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    // NAVIGATION PROPERTIES!!!!!
    public Customer Customer { get; set; } = null!;

    public List<OrderDetail> OrderDetails { get; set; } = new();
}