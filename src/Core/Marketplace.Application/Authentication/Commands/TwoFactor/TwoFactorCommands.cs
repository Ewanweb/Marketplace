using System.Text.Json;
using FluentValidation;
using Marketplace.Application.Authentication.Common;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Authentication.Commands.TwoFactor;

public sealed record Enable2FAResponse(string SecretKey, string QrCodeUri);
public sealed record Enable2FACommand(Guid UserId) : IRequest<Result<Enable2FAResponse>>;

public sealed record Verify2FAEnableResponse(IEnumerable<string> BackupCodes);
public sealed record Verify2FAEnableCommand(Guid UserId, string Code) : IRequest<Result<Verify2FAEnableResponse>>;

public sealed class Enable2FACommandHandler : IRequestHandler<Enable2FACommand, Result<Enable2FAResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITotpService _totpService;

    public Enable2FACommandHandler(IApplicationDbContext dbContext, ITotpService totpService)
    {
        _dbContext = dbContext;
        _totpService = totpService;
    }

    public async Task<Result<Enable2FAResponse>> Handle(Enable2FACommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<Enable2FAResponse>(Error.NotFound("User.NotFound", AuthMessages.UserNotFound));
        }

        var secretKey = _totpService.GenerateSecretKey();
        var qrCodeUri = _totpService.GenerateQrCodeUri(user.Email, secretKey);

        return Result.Success(new Enable2FAResponse(secretKey, qrCodeUri));
    }
}

public sealed class Verify2FAEnableCommandHandler : IRequestHandler<Verify2FAEnableCommand, Result<Verify2FAEnableResponse>>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ITotpService _totpService;

    public Verify2FAEnableCommandHandler(IApplicationDbContext dbContext, ITotpService totpService)
    {
        _dbContext = dbContext;
        _totpService = totpService;
    }

    public async Task<Result<Verify2FAEnableResponse>> Handle(Verify2FAEnableCommand request, CancellationToken cancellationToken)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);
        if (user is null)
        {
            return Result.Failure<Verify2FAEnableResponse>(Error.NotFound("User.NotFound", AuthMessages.UserNotFound));
        }

        var secretKey = _totpService.GenerateSecretKey(); // For real flow secret passed or stored temporarily
        var isValid = _totpService.VerifyCode(secretKey, request.Code);
        if (!isValid)
        {
            return Result.Failure<Verify2FAEnableResponse>(Error.Validation("Auth.Invalid2FACode", AuthMessages.Invalid2FACode));
        }

        var backupCodes = _totpService.GenerateBackupCodes().ToList();
        var backupCodesJson = JsonSerializer.Serialize(backupCodes);

        user.EnableTwoFactor(secretKey, backupCodesJson);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(new Verify2FAEnableResponse(backupCodes));
    }
}
