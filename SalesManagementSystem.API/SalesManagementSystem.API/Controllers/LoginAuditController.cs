using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SalesManagementSystem.Data;

namespace SalesManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin")]
public class LoginAuditController : ControllerBase
{
    private readonly AppDbContext _context;

    public LoginAuditController(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves login audit records.
    /// </summary>
    /// <returns>A list of login audit records.</returns>
    /// <response code="200">Login audit records retrieved successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">SuperAdmin privileges required.</response>
    [HttpGet]
    public IActionResult GetAll(
    string? username,
    bool? isSuccess,
    DateTime? fromDate,
    DateTime? toDate,
    int pageNumber = 1,
    int pageSize = 10)
    {
        // Filters, paginates, and returns login audit logs sorted by date descending

        var query = _context.LoginAudits.AsQueryable();

        if (!string.IsNullOrWhiteSpace(username))
        {
            query = query.Where(x => x.Username.Contains(username));
        }

        if (isSuccess.HasValue)
        {
            query = query.Where(x => x.IsSuccess == isSuccess.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.LoginTime >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.LoginTime <= toDate.Value);
        }

        pageNumber = Math.Max(pageNumber, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var result = query
            .OrderByDescending(x => x.LoginTime)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Ok(result);
    }
}