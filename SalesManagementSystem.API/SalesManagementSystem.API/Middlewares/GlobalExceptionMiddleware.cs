using System.Net;
using System.Text.Json;
using SalesManagementSystem.API.Exceptions;

namespace SalesManagementSystem.API.Middlewares;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Catches any thrown exceptions during request execution to log and handle them centrally

        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            _logger.LogWarning(ex, ex.Message);

            await HandleAppExceptionAsync(context, ex);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception occurred.");

            await HandleGenericExceptionAsync(context);
        }
    }

    private static Task HandleAppExceptionAsync(HttpContext context, AppException exception)
    {
        // Formats and returns custom application specific errors as structured JSON responses

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception.StatusCode;

        var response = new
        {
            success = false,
            message = exception.Message,
            errors = (object?)null
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static Task HandleGenericExceptionAsync(HttpContext context)
    {
        // Formats and returns unhandled server errors as structured 500 Internal Server Error JSON responses

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            success = false,
            message = "An unexpected error occurred.",
            errors = (object?)null
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}