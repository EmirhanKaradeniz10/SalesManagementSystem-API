using System.ComponentModel.DataAnnotations;

namespace SalesManagementSystem.API.DTOs
{
    public class UpdateUserRoleDto
    {
        [Required]
        public string Role { get; set; } = string.Empty;
    }
}
