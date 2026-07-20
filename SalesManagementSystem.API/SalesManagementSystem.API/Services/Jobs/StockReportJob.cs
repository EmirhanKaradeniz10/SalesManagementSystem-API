using Microsoft.EntityFrameworkCore;
using SalesManagementSystem.API.Services.Email;
using SalesManagementSystem.Data;

namespace SalesManagementSystem.API.Services.Jobs;

public class StockReportJob
{
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;
    private readonly IConfiguration _configuration;

    public StockReportJob(
        AppDbContext context,
        EmailService emailService,
        IConfiguration configuration)
    {
        _context = context;
        _emailService = emailService;
        _configuration = configuration;
    }

    public async Task Execute()
    {
        var lowStockProducts = await _context.Products
            .Where(p => p.Stock < 10)
            .ToListAsync();

        if (!lowStockProducts.Any())
        {
            return;
        }

        var adminEmail = _configuration["Email:AdminEmail"]!;

        await _emailService.SendLowStockReportAsync(
            adminEmail,
            lowStockProducts);
    }
}