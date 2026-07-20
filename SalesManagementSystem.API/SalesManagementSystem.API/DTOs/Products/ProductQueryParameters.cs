namespace SalesManagementSystem.API.DTOs.Products;

public class ProductQueryParameters
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public string? Search { get; set; }

    public string? Category { get; set; }

    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }

    public string? SortBy { get; set; }

    public bool IsDescending { get; set; } = false;
}