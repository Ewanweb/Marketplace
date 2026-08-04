using Marketplace.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Marketplace.Identity.Services;

public sealed class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailVerificationAsync(string toEmail, string verificationToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending Email Verification to {Email}. Token: {Token}", toEmail, verificationToken);
        return Task.CompletedTask;
    }

    public Task SendPasswordResetAsync(string toEmail, string resetToken, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending Password Reset to {Email}. Token: {Token}", toEmail, resetToken);
        return Task.CompletedTask;
    }
}
