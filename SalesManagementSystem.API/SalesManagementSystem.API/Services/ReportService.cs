using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using SalesManagementSystem.API.DTOs.Reports;
using SalesManagementSystem.Data;

public class ReportService
{
    private readonly AppDbContext _context;

    public ReportService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<OrderReportDto>> GetOrderReportAsync()
    {
        // EF Core, Stored Procedure'ü çalıştırır ve 
        // gelen kolonları otomatik olarak OrderReportDto nesnesine map eder.
        return await _context.Database
            .SqlQuery<OrderReportDto>($"EXEC dbo.GetOrderSummary")
            .ToListAsync();
    }
}