using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.API.DTOs.Auth;

public class RegisterRequestDto
{
    [Required]
    public string Username { get; set; } = null!;

    [Required]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}