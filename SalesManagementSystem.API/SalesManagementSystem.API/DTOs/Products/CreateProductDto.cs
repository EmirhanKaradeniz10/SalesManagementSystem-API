using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.API.DTOs.Products;

public class CreateProductDto
{
    [Required]
    public string Name { get; set; } = string.Empty;

    [Range(1, double.MaxValue)]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue)]
    public int Stock { get; set; }

    public int? CategoryId { get; set; }
}