using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace Marketplace.API.Controllers;

public class LogsController : ApiControllerBase
{
    private readonly Serilog.ILogger _logger = Log.ForContext<LogsController>();

    /// <summary>
    /// Centralized endpoint to receive and log frontend Flutter web errors, stacktraces & warnings to Seq.
    /// </summary>
    [HttpPost("client")]
    public IActionResult LogClientError([FromBody] ClientLogRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        var userAgent = Request.Headers.UserAgent.ToString();

        var enrichedLogger = _logger
            .ForContext("SourceContext", "Frontend.Flutter")
            .ForContext("UserId", userId)
            .ForContext("UserAgent", userAgent)
            .ForContext("ClientLogLevel", request.Level ?? "Error")
            .ForContext("StackTrace", request.StackTrace ?? "N/A");

        if (string.Equals(request.Level, "Warning", StringComparison.OrdinalIgnoreCase))
        {
            enrichedLogger.Warning("[Flutter Client Warning] {Message} (Route: {Route})", request.Message, request.Route);
        }
        else
        {
            enrichedLogger.Error("[Flutter Client Error] {Message} (Route: {Route})", request.Message, request.Route);
        }

        return Ok(new { isSuccess = true });
    }
}

public record ClientLogRequest(
    string Message,
    string? StackTrace,
    string? Level,
    string? Route);
