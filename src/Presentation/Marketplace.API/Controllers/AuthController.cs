using Marketplace.Application.Authentication.Commands.LoginUser;
using Marketplace.Application.Authentication.Commands.Logout;
using Marketplace.Application.Authentication.Commands.RefreshToken;
using Marketplace.Application.Authentication.Commands.RegisterUser;
using Marketplace.Application.Authentication.Commands.TwoFactor;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

public class AuthController : ApiControllerBase
{
    /// <summary>
    /// Registers a new user account.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserCommand command, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Authenticates a user and returns JWT and Refresh Token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userAgent = Request.Headers.UserAgent.ToString();

        var command = new LoginCommand(request.Email, request.Password, ipAddress, userAgent);
        var result = await Sender.Send(command, cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Rotates refresh token and generates a new Access Token.
    /// </summary>
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new RefreshTokenCommand(request.RefreshToken, ipAddress);

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Revokes current refresh token (logout).
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var command = new LogoutCommand(request.RefreshToken, ipAddress);

        var result = await Sender.Send(command, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Enables 2FA TOTP setup for user.
    /// </summary>
    [HttpPost("2fa/enable")]
    public async Task<IActionResult> Enable2FA([FromBody] Enable2FARequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new Enable2FACommand(request.UserId), cancellationToken);
        return HandleResult(result);
    }
}

public record LoginRequest(string Email, string Password);
public record RefreshTokenRequest(string RefreshToken);
public record Enable2FARequest(Guid UserId);
