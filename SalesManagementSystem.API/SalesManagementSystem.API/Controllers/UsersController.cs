using DocumentFormat.OpenXml.Office2010.PowerPoint;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SalesManagementSystem.API.DTOs;
using SalesManagementSystem.Data;

namespace SalesManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly AppDbContext _context;

    public UsersController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all users.
    /// </summary>
    /// <returns>List of all users.</returns>
    /// <response code="200">Users retrieved successfully.</response>
    /// <response code="403">Only SuperAdmin can access this endpoint.</response>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin")]
    public IActionResult GetAll()
    {
        // Retrieves a list of all users mapped to list DTOs

        var users = _context.Users
            .Select(u => new UserListDto
            {
                Id = u.Id,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role
            })
            .ToList();

        return Ok(new
        {
            success = true,
            message = "Users retrieved successfully.",
            data = users
        });
    }

    /// <summary>
    /// Changes a user's role.
    /// </summary>
    /// <param name="id">User identifier.</param>
    /// <param name="dto">New role information (only User/Admin).</param>
    /// <returns>Updated user role.</returns>
    /// <response code="200">Role updated successfully.</response>
    /// <response code="400">Invalid role.</response>
    /// <response code="403">Only SuperAdmin can change roles.</response>
    /// <response code="404">User not found.</response>
    [HttpPatch("{id}/role")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> UpdateRole(int id, UpdateUserRoleDto dto)
    {
        // Updates a specific user's system role after performing validation checks

        var user = await _context.Users.FindAsync(id);

        if (user == null)
        {
            return NotFound(new
            {
                success = false,
                message = "User not found"
            });
        }

        // Prevents modification of the SuperAdmin role for system security
        if (user.Role == "SuperAdmin")
        {
            return StatusCode(StatusCodes.Status403Forbidden, new
            {
                success = false,
                message = "SuperAdmin role cannot be modified."
            });
        }

        // Ensures only 'Admin' and 'User' roles are accepted as valid inputs
        if (dto.Role != "Admin" && dto.Role != "User")
        {
            return BadRequest(new
            {
                success = false,
                message = "Role must be either 'Admin' or 'User'."
            });
        }

        user.Role = dto.Role;

        await _context.SaveChangesAsync();

        return Ok(new
        {
            success = true,
            message = "User role updated successfully.",
            data = new
            {
                user.Id,
                user.Username,
                user.Role
            }
        });
    }
}