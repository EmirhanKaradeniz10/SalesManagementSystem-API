using BCrypt.Net;

namespace SalesManagementSystem.API.Services.Auth;

public class PasswordService
{
    public string HashPassword(string password)
    {
        // Hashes a plain-text password using the BCrypt algorithm

        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        // Verifies if a plain-text password matches a previously hashed password

        return BCrypt.Net.BCrypt.Verify(password, passwordHash);
    }
}