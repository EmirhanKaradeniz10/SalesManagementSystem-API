using Microsoft.EntityFrameworkCore;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models;
using SalesManagementSystem.API.Services.Auth;

namespace SalesManagementSystem.API.Services;

public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        // Checks if a SuperAdmin exists and creates one using settings if missing

        using var scope = serviceProvider.CreateScope();

        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var passwordService = scope.ServiceProvider.GetRequiredService<PasswordService>();

        var superAdminExists = await context.Users
            .AnyAsync(x => x.Role == "SuperAdmin");

        if (superAdminExists)
            return;

        var username = configuration["SuperAdmin:Username"];
        var email = configuration["SuperAdmin:Email"];
        var password = configuration["SuperAdmin:Password"];

        if (string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password))
        {
            throw new Exception("SuperAdmin configuration is missing.");
        }

        var customer = new Customer
        {
            Name = username
        };

        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var superAdmin = new User
        {
            Username = username,
            Email = email,
            PasswordHash = passwordService.HashPassword(password),
            Role = "SuperAdmin",
            CustomerId = customer.Id
        };

        context.Users.Add(superAdmin);

        await context.SaveChangesAsync();
    }
}