namespace SalesManagementSystem.API.Middleware;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        // Logs the incoming HTTP request method and path to the console

        Console.WriteLine(
            $"Request: {context.Request.Method} {context.Request.Path}");

        await _next(context);
    }
}