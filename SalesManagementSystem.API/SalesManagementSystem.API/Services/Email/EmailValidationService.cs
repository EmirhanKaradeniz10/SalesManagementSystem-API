using System.Net;

namespace SalesManagementSystem.API.Services;

public class EmailValidationService
{
    public async Task<bool> DomainExistsAsync(string email)
    {
        // Extracts the domain from the email and checks if it has a valid DNS record

        try
        {
            var domain = email.Split('@').Last();

            await Dns.GetHostEntryAsync(domain);

            return true;
        }
        catch
        {
            return false;
        }
    }
}