using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SalesManagementSystem.API.DTOs;
using SalesManagementSystem.API.DTOs.Products;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models;

namespace SalesManagementSystem.API.Services;

public class ProductService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public ProductService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // ---------------- CACHE VERSION ----------------

    private int GetCacheVersion()
    {
        // Retrieves the current global cache version from memory cache

        return _cache.Get<int>(ProductCacheKeys.Version);
    }

    private string GetProductsCacheKey(int page, int size, string? search, string? category, decimal? min, decimal? max, string? sort, bool desc)
    {
        // Generates a unique cache key based on filtering and pagination parameters

        var version = GetCacheVersion();

        return $"products_v{version}" +
       $"_p{page}" +
       $"_s{size}" +
       $"_q{Normalize(search)}" +
       $"_cat{Normalize(category)}" +
       $"_min{min?.ToString() ?? "null"}" +
       $"_max{max?.ToString() ?? "null"}" +
       $"_sort{sort ?? "id"}" +
       $"_desc{desc}";
    }

    public void InvalidateProductsCache()
    {
        // Invalidates the current product cache by incrementing the cache version

        var version = GetCacheVersion();
        _cache.Set(ProductCacheKeys.Version, version + 1);
    }

    // ---------------- GET FILTERED ----------------

    public IEnumerable<ProductDto> GetFiltered(
        int pageNumber,
        int pageSize,
        string? search,
        string? category,
        decimal? minPrice,
        decimal? maxPrice,
        string? sortBy,
        bool isDescending)
    {
        // Fetches filtered and paginated products from cache or queries the database

        var cacheKey = GetProductsCacheKey(pageNumber, pageSize, search, category, minPrice, maxPrice, sortBy, isDescending);
        Console.WriteLine("CACHE KEY => " + cacheKey);

        if (_cache.TryGetValue(cacheKey, out List<ProductDto>? cachedProducts))
        {
            Console.WriteLine("CACHE HIT");
            return cachedProducts!;
        }

        var query = _context.Products
            .Include(p => p.Category)
            .AsQueryable();

        // SEARCH
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.Name.Contains(search));

        //CATEGORY SEARCH
        if (!string.IsNullOrWhiteSpace(category))
        {
            if (int.TryParse(category, out var categoryId))
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }
            else
            {
                query = query.Where(p => p.Category!.Name.Contains(category));
            }
        }

        // PRICE FILTER
        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice.Value);

        // SORT
        if (string.IsNullOrWhiteSpace(sortBy))
        {
            query = query.OrderBy(p => p.Id);
        }
        else
        {
            query = sortBy.ToLower() switch
            {
                "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => isDescending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                "stock" => isDescending ? query.OrderByDescending(p => p.Stock) : query.OrderBy(p => p.Stock),
                _ => query.OrderBy(p => p.Id)
            };
        }

        var result = query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

        var response = result.Select(p => new ProductDto
        {
            Id = p.Id,
            Name = p.Name,
            Price = p.Price,
            Stock = p.Stock,
            CategoryId = p.CategoryId,
            CategoryName = p.Category?.Name
        }).ToList();

        _cache.Set(cacheKey, response, TimeSpan.FromMinutes(2));

        return response;
    }

    // ---------------- CRUD ----------------

    public Product Add(Product product)
    {
        // Adds a new product to the database and resets the product cache

        _context.Products.Add(product);
        _context.SaveChanges();

        InvalidateProductsCache();

        return product;
    }

    //Update
    public Product? Update(int id, Product updated)
    {
        // Updates an existing product's fields and resets the product cache

        var product = _context.Products.Find(id);

        if (product == null)
            return null;

        product.Name = updated.Name;
        product.Price = updated.Price;
        product.Stock = updated.Stock;
        product.CategoryId = updated.CategoryId;

        _context.SaveChanges();

        InvalidateProductsCache();

        return product;
    }

    //Delete
    public bool Delete(int id)
    {
        // Removes a product from the database by ID and resets the product cache

        var product = _context.Products.Find(id);

        if (product == null)
            return false;

        _context.Products.Remove(product);
        _context.SaveChanges();

        InvalidateProductsCache();

        return true;
    }

    // ---------------- HELPERS ----------------

    public bool CategoryExists(int? categoryId)
    {
        // Validates whether a category identifier exists in the database

        if (categoryId == null)
            return true;

        return _context.Categories.Any(c => c.Id == categoryId);
    }

    public bool ProductNameExists(string name)
    {
        // Checks if a product name is already taken in the system

        return _context.Products.Any(p => p.Name == name);
    }

    public List<Product> GetAll()
    {
        // Returns a list of all products including their category info

        return _context.Products.Include(p => p.Category).ToList();
    }

    public Product? GetById(int id)
    {
        // Finds and returns a single product by its unique identifier

        return _context.Products
            .Include(p => p.Category)
            .FirstOrDefault(p => p.Id == id);
    }

    private string Normalize(string? value)
    {
        // Cleans and formats query values to create consistent cache keys

        return string.IsNullOrWhiteSpace(value) ? "all" : value.Trim().ToLower();
    }
}