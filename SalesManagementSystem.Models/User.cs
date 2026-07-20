namespace SalesManagementSystem.Models;

public class User
{
    public int Id { get; set; }

    public string Username { get; set; } = null!;

    public string Email { get; set; } = null!;

    public int? CustomerId { get; set; }

    public Customer? Customer { get; set; }

    public bool IsActive { get; set; } = true;

    public string PasswordHash { get; set; } = null!;


    public string Role { get; set; } = "User";

    public int FailedLoginAttempts { get; set; } = 0;

    public DateTime? LockoutEnd { get; set; }

    //Refresh token yapıyorum.
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }

}