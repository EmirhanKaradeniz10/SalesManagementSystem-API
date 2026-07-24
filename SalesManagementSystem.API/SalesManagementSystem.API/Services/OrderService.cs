using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using SalesManagementSystem.API.DTOs.Orders;
using SalesManagementSystem.API.Exceptions;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models;
using SalesManagementSystem.Models.Enums;

namespace SalesManagementSystem.API.Services;

public class OrderService
{
    private readonly AppDbContext _context;
    private readonly IMemoryCache _cache;

    public OrderService(AppDbContext context, IMemoryCache cache)
    {
        _context = context;
        _cache = cache;
    }

    // Increments the cache version so that product updates invalidate the existing cache
    private void InvalidateProductsCache()
    {
        var version = _cache.Get<int>(ProductCacheKeys.Version);

        _cache.Set(ProductCacheKeys.Version, version + 1);
    }

    public async Task<int> CreateOrderAsync(
    CreateOrderRequestDto dto,
    string username)
    {
        if (dto.Products == null || dto.Products.Count == 0)
            throw new AppException(
                "At least one product is required.",
                400);

        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Username == username);

        if (user == null)
            throw new AppException("User not found", 404);

        if (user.CustomerId == null)
            throw new AppException(
                "Customer account not found",
                404);

        var order = new Order
        {
            CustomerId = user.CustomerId.Value,
            OrderDate = DateTime.UtcNow,
            OrderDetails = new List<OrderDetail>()
        };

        foreach (var item in dto.Products)
        {
            if (item.Quantity <= 0)
                throw new AppException(
                    "Quantity must be greater than zero.",
                    400);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == item.ProductId);

            if (product == null)
                throw new AppException(
                    $"Product not found: {item.ProductId}",
                    400);

            if (product.Stock < item.Quantity)
                throw new AppException(
                    $"Insufficient stock for product: {product.Name}",
                    400);

            product.Stock -= item.Quantity;

            order.OrderDetails.Add(new OrderDetail
            {
                ProductId = product.Id,
                Quantity = item.Quantity
            });
        }

        _context.Orders.Add(order);

        // EF Core automatically executes this SaveChanges
        // operation inside a transaction.
        await _context.SaveChangesAsync();

        InvalidateProductsCache();

        return order.Id;
    }

    public async Task<int> GetAllOrdersCountAsync()
    {
        // Returns the total count of all orders recorded in the database

        return await _context.Orders.CountAsync();
    }

    public async Task<OrderResponseDto?> GetOrderByIdAsync(int id, string username, bool isAdmin)
    {
        // Retrieves a specific order by ID, applying role-based data access restrictions

        int? customerId = null;

        if (!isAdmin)
        {
            customerId = await _context.Users
                .Where(u => u.Username == username)
                .Select(u => u.CustomerId)
                .FirstOrDefaultAsync();

            if (customerId == null)
                throw new AppException("Customer not found", 404);
        }

        var query = _context.Orders
                        .Include(o => o.Customer)
                        .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                        .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(o => o.CustomerId == customerId);
        }

        var order = await query.FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return null;

        return new OrderResponseDto
        {
            Id = order.Id,
            CustomerName = order.Customer.Name,
            OrderDate = order.OrderDate,
            Status = order.Status,
            TotalPrice = order.OrderDetails.Sum(x =>
                x.Quantity * x.Product.Price),

            Items = order.OrderDetails.Select(x => new OrderDetailResponseDto
            {
                ProductId = x.ProductId,
                ProductName = x.Product.Name,
                Quantity = x.Quantity,
                Price = x.Product.Price
            }).ToList()
        };
    }

    
    public async Task CancelOrderAsync(int orderId, string username, bool isAdmin)
    {
        // Cancels a specific order, restores product stock levels, and invalidates product cache

        int? customerId = null;

        if (!isAdmin)
        {
            customerId = await _context.Users
                .Where(u => u.Username == username)
                .Select(u => u.CustomerId)
                .FirstOrDefaultAsync();

            if (customerId == null)
                throw new AppException("Customer not found", 404);
        }

        var query = _context.Orders
            .Include(o => o.OrderDetails)
            .AsQueryable();

        if (!isAdmin)
        {
            query = query.Where(o => o.CustomerId == customerId);
        }

        var order = await query.FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new AppException("Order not found", 400);

        if (order.Status == OrderStatus.Cancelled)
            throw new AppException("Order already cancelled", 400);

        if (order.Status == OrderStatus.Completed)
            throw new AppException("Completed order cannot be cancelled", 400);

        foreach (var item in order.OrderDetails)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == item.ProductId);

            if (product != null)
            {
                product.Stock += item.Quantity;
            }
        }

        order.Status = OrderStatus.Cancelled;

        await _context.SaveChangesAsync();

        InvalidateProductsCache();
    }

    public async Task CompleteOrderAsync(int orderId)
    {
        // Finalizes an order status to completed and locks it from further modifications

        var order = await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
            throw new AppException("Order not found", 404);

        if (order.Status == OrderStatus.Cancelled)
            throw new AppException("Cancelled order cannot be completed", 400);

        if (order.Status == OrderStatus.Completed)
            throw new AppException("Order already completed", 400);

        order.Status = OrderStatus.Completed;

        await _context.SaveChangesAsync();
    }

    public async Task<List<OrderResponseDto>> GetFilteredOrdersAsync(OrderQueryParameters query, 
                                                                        string username, bool isAdmin)
    {
        // Fetches a filtered and paginated list of orders mapped to response DTOs

        int? customerId = null;

        if (!isAdmin)
        {
            customerId = await _context.Users
                .Where(u => u.Username == username)
                .Select(u => u.CustomerId)
                .FirstOrDefaultAsync();

            if (customerId == null)
                throw new AppException("Customer not found", 404);
        }

        var orders = _context.Orders
            .Include(o => o.Customer)
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
            .AsQueryable();

        if (!isAdmin)
        {
            orders = orders.Where(o => o.CustomerId == customerId);
        }

        if (query.Status.HasValue)
        {
            orders = orders.Where(o => o.Status == query.Status.Value);
        }

        if (query.CustomerId.HasValue)
        {
            orders = orders.Where(o => o.CustomerId == query.CustomerId.Value);
        }

        if (query.FromDate.HasValue)
        {
            orders = orders.Where(o => o.OrderDate >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            orders = orders.Where(o => o.OrderDate <= query.ToDate.Value);
        }

        orders = orders
            .OrderByDescending(o => o.Id)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize);

        // Sorts the filtered results in descending order by order ID

        return await orders
            .Select(order => new OrderResponseDto
            {
                Id = order.Id,
                CustomerName = order.Customer.Name,
                OrderDate = order.OrderDate,
                Status = order.Status,

                TotalPrice = order.OrderDetails.Sum(x =>
                    x.Quantity * x.Product.Price),

                Items = order.OrderDetails.Select(x => new OrderDetailResponseDto
                {
                    ProductId = x.ProductId,
                    ProductName = x.Product.Name,
                    Quantity = x.Quantity,
                    Price = x.Product.Price
                }).ToList()
            })
            .ToListAsync();
    }

}