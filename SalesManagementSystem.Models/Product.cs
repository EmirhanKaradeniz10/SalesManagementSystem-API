using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.Models;

public class Product
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Name { get; set; }

    public decimal Price { get; set; }

    public int Stock { get; set; }

    public int? CategoryId { get; set; }

    // NAVIGATION PROPERTY
    
    public Category? Category { get; set; }
}