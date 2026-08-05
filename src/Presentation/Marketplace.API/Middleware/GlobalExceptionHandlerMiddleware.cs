using System.Net;
using System.Security.Claims;
using System.Text.Json;
using Marketplace.Shared.Results;

namespace Marketplace.API.Middleware;

public sealed class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);

            // Log warnings for 4xx responses (Bad Request, Unauthorized, Not Found, etc.)
            if (context.Response.StatusCode >= 400 && context.Response.StatusCode < 500)
            {
                var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
                _logger.LogWarning(
                    "HTTP {Method} {Path} responded with Status Code {StatusCode} for User {UserId}",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    userId);
            }
        }
        catch (Exception ex)
        {
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
            _logger.LogError(
                ex,
                "Unhandled exception caught on HTTP {Method} {Path} for User {UserId}: {Message}",
                context.Request.Method,
                context.Request.Path,
                userId,
                ex.Message);

            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var result = new { isSuccess = false, error = new { code = "Server.InternalError", message = "An unexpected internal server error occurred." } };
        var response = JsonSerializer.Serialize(result);

        return context.Response.WriteAsync(response);
    }
}
