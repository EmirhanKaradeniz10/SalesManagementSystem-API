using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SalesManagementSystem.API.DTOs;
using SalesManagementSystem.API.DTOs.Orders;
using SalesManagementSystem.API.Services;
using SalesManagementSystem.Models;

namespace SalesManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    /// <summary>
    /// Creates a new order.
    /// </summary>
    /// <param name="dto">Order information including customer and products.</param>
    /// <returns>The identifier of the created order.</returns>
    /// <response code="200">Order created successfully.</response>
    /// <response code="400">Validation failed or invalid request.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Access denied.</response>
    [HttpPost]
    [Authorize(Roles = "User,Admin,SuperAdmin")]
    [EnableRateLimiting("orders-create")]
    public async Task<IActionResult> CreateOrder(CreateOrderRequestDto dto)
    {
        // Validates the authenticated user and creates a new order in the system

        var username = User.Identity?.Name;

        if (string.IsNullOrEmpty(username))
        {
            return Unauthorized(new ApiResponse<string>
            {
                Success = false,
                Message = "Invalid user",
                Data = null
            });
        }

        var orderId = await _orderService.CreateOrderAsync(dto, username);

        var allOrders = await _orderService.GetAllOrdersCountAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Order created successfully",
            Data = new
            {
                OrderId = orderId,
                TotalOrdersInDb = allOrders
            }
        });
    }

    /// <summary>
    /// Retrieves a filtered and paginated list of orders.
    /// </summary>
    /// <param name="query">Filtering and pagination parameters.</param>
    /// <returns>A filtered list of orders.</returns>
    /// <response code="200">Orders retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Access denied.</response>
    [HttpGet]
    [Authorize(Roles = "User,Admin,SuperAdmin")]
    [EnableRateLimiting("orders-read")]
    public async Task<IActionResult> GetOrders([FromQuery] OrderQueryParameters query)
    {
        // Fetches a filtered, paginated list of orders based on user permissions

        var username = User.Identity?.Name;
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin"); 

        var orders = await _orderService.GetFilteredOrdersAsync(
            query,
            username!,
            isAdmin);

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Orders retrieved successfully",
            Data = orders
        });
    }

    /// <summary>
    /// Retrieves an order by its identifier.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <returns>The requested order.</returns>
    /// <response code="200">Order retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Access denied.</response>
    /// <response code="404">Order not found.</response>
    [HttpGet("{id}")]
    [Authorize(Roles = "User,Admin,SuperAdmin")]
    [EnableRateLimiting("orders-read")]
    public async Task<IActionResult> GetOrder(int id)
    {
        // Retrieves the details of a specific order by its unique identifier

        var username = User.Identity?.Name;
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        var order = await _orderService.GetOrderByIdAsync(id, username!, isAdmin);

        if (order == null)
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Order not found",
                Data = null
            });

        return Ok(new ApiResponse<OrderResponseDto>
        {
            Success = true,
            Message = "Order retrieved successfully",
            Data = order
        });
    }

    /// <summary>
    /// Cancels an existing order.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <returns>Confirmation of the cancellation operation.</returns>
    /// <response code="200">Order cancelled successfully.</response>
    /// <response code="400">Order cannot be cancelled.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Access denied.</response>
    /// <response code="404">Order not found.</response>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "User,Admin,SuperAdmin")]
    public async Task<IActionResult> CancelOrder(int id)
    {
        // Cancels a specific order and restores the related product stock

        var username = User.Identity?.Name;
        var isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        await _orderService.CancelOrderAsync(id, username!, isAdmin);

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Order cancelled successfully",
            Data = null
        });
    }


    /// <summary>
    /// Marks an order as completed.
    /// </summary>
    /// <param name="id">Order identifier.</param>
    /// <returns>Confirmation of the completion operation.</returns>
    /// <response code="200">Order completed successfully.</response>
    /// <response code="400">Order cannot be completed.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Administrator privileges required.</response>
    /// <response code="404">Order not found.</response>
    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> CompleteOrder(int id)
    {
        // Updates the status of a specific order to completed

        await _orderService.CompleteOrderAsync(id);

        return Ok(new ApiResponse<string>
        {
            Success = true,
            Message = "Order completed successfully",
            Data = null
        });
    }
}