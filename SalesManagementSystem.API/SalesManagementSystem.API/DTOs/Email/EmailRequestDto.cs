namespace SalesManagementSystem.API.DTOs.Email;

public class EmailRequestDto
{
    public string To { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string HtmlBody { get; set; } = null!;
}