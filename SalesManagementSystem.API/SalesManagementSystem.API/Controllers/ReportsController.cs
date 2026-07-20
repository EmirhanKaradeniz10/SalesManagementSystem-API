using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SalesManagementSystem.API.Services;

namespace SalesManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly ReportService _service;

    public ReportsController(ReportService service)
    {
        _service = service;
    }


    /// <summary>
    /// Generates the orders report.
    /// </summary>
    /// <returns>Order report data.</returns>
    /// <response code="200">Report generated successfully.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Administrator privileges required.</response>
    [HttpGet("orders")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [EnableRateLimiting("reports")]
    public async Task<IActionResult> GetOrders()
    {
        // Generates and returns a comprehensive report summarizing order statistics

        var data = await _service.GetOrderReportAsync();

        return Ok(new
        {
            success = true,
            message = "Order report retrieved",
            data
        });
    }
}