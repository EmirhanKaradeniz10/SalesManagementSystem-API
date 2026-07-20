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
        // Executes a stored procedure to retrieve and map custom database order summary data

        var connection = _context.Database.GetDbConnection();

        await using var command = connection.CreateCommand();
        command.CommandText = "GetOrderSummary";
        command.CommandType = System.Data.CommandType.StoredProcedure;

        await connection.OpenAsync();

        await using var reader = await command.ExecuteReaderAsync();

        var result = new List<OrderReportDto>();

        while (await reader.ReadAsync())
        {
            result.Add(new OrderReportDto
            {
                OrderId = reader.GetInt32(0),
                CustomerName = reader.GetString(1),
                OrderDate = reader.GetDateTime(2),
                ProductName = reader.GetString(3),
                Quantity = reader.GetInt32(4),
                Price = reader.GetDecimal(5),
                TotalPrice = reader.GetDecimal(6)
            });
        }

        return result;
    }
}