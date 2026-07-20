namespace SalesManagementSystem.API.DTOs.Products;

public class ImportErrorDto
{
    public string? Name { get; set; }

    public decimal? Price { get; set; }

    public int? Stock { get; set; }

    public int? CategoryId { get; set; }

    public string Error { get; set; } = string.Empty;
}