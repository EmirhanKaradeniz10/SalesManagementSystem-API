using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesManagementSystem.API.DTOs;
using SalesManagementSystem.API.DTOs.Customers;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models;
using SalesManagementSystem.Models.Enums;

namespace SalesManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _context;

    public CustomersController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all customers.
    /// </summary>
    /// <returns>A list of all customers.</returns>
    /// <response code="200">Customers retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Access denied.</response>
    [HttpGet]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public IActionResult GetAll()
    {
        // Retrieves a list of all customers mapped to DTOs

        var customers = _context.Customers
            .Select(c => new CustomerDto
            {
                Id = c.Id,
                Name = c.Name
            })
            .ToList();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = "Customers retrieved successfully",
            Data = customers
        });
    }

    /// <summary>
    /// Activates or deactivates a customer account.
    /// </summary>
    /// <param name="id">Customer identifier.</param>
    /// <param name="dto">Customer status information.</param>
    /// <returns>Customer account status.</returns>
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateCustomerStatusDto dto)
    {
        var customer = await _context.Customers
            .Include(c => c.Users)
            .Include(c => c.Orders)
                .ThenInclude(o => o.OrderDetails)
                    .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (customer == null)
        {
            return NotFound(new ApiResponse<string>
            {
                Success = false,
                Message = "Customer not found",
                Data = null
            });
        }

        var user = customer.Users.FirstOrDefault();

        if (user == null)
        {
            return BadRequest(new ApiResponse<string>
            {
                Success = false,
                Message = "Customer has no user account.",
                Data = null
            });
        }

        // Prevents an Admin from deactivating a SuperAdmin account
        if (User.IsInRole("Admin") && user.Role == "SuperAdmin")
        {
            return Forbid();
        }

        customer.IsActive = dto.IsActive;
        user.IsActive = dto.IsActive;

        // If the account is being deactivated, cancel pending orders and restore product stock
        if (!dto.IsActive)
        {
            var pendingOrders = customer.Orders
                .Where(o => o.Status == OrderStatus.Pending);

            foreach (var order in pendingOrders)
            {
                foreach (var detail in order.OrderDetails)
                {
                    detail.Product.Stock += detail.Quantity;
                }

                order.Status = OrderStatus.Cancelled;
                order.IsCancelled = true;
            }

            // Optionally revokes the user's refresh token for security
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
        }

        await _context.SaveChangesAsync();

        return Ok(new ApiResponse<object>
        {
            Success = true,
            Message = dto.IsActive
                ? "Customer activated successfully."
                : "Customer deactivated successfully.",
            Data = new
            {
                customer.Id,
                customer.Name,
                customer.IsActive
            }
        });
    }
}