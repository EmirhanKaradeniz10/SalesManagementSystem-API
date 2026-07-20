namespace SalesManagementSystem.API.DTOs.Products;

public class ProductImportDto
{
    public string? Name { get; set; }

    public decimal? Price { get; set; }

    public int? Stock { get; set; }

    public int? CategoryId { get; set; }
}