using Resend;
using SalesManagementSystem.API.DTOs.Email;
using SalesManagementSystem.Models;
using System.Text;

namespace SalesManagementSystem.API.Services.Email;

public class EmailService
{
    private readonly IResend _resend;

    public EmailService(IResend resend)
    {
        _resend = resend;
    }

    public async Task SendAsync(EmailRequestDto dto)
    {
        // Sends an email message through the Resend client using a default sender address

        var message = new EmailMessage
        {
            From = "Sales Management <onboarding@resend.dev>",
            To = dto.To,
            Subject = dto.Subject,
            HtmlBody = dto.HtmlBody
        };

        await _resend.EmailSendAsync(message);
    }

    public async Task SendLowStockReportAsync(string to, List<Product> products)
    {
        // Generates an HTML table report for low-stock products and dispatches the email

        var html = new StringBuilder();

        html.Append("<h2>Low Stock Report</h2>");
        html.Append("<p>The following products are running low on stock:</p>");

        html.Append(@"
        <table border='1' cellpadding='6' cellspacing='0'>
            <tr>
                <th>Product</th>
                <th>Stock</th>
            </tr>");

        foreach (var product in products)
        {
            html.Append($@"
            <tr>
                <td>{product.Name}</td>
                <td>{product.Stock}</td>
            </tr>");
        }

        html.Append("</table>");

        await SendAsync(new EmailRequestDto
        {
            To = to,
            Subject = "Low Stock Report",
            HtmlBody = html.ToString()
        });
    }
}