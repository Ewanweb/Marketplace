using FluentValidation;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Domain.Entities;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Admin.Commands.UpdateSiteSetting;

public sealed record UpdateSiteSettingCommand(string Key, string Value) : IRequest<Result>;

public sealed class UpdateSiteSettingCommandValidator : AbstractValidator<UpdateSiteSettingCommand>
{
    public UpdateSiteSettingCommandValidator()
    {
        RuleFor(v => v.Key).NotEmpty();
        RuleFor(v => v.Value).NotNull();
    }
}

public sealed class UpdateSiteSettingCommandHandler : IRequestHandler<UpdateSiteSettingCommand, Result>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateSiteSettingCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(UpdateSiteSettingCommand request, CancellationToken cancellationToken)
    {
        if (request.Key == "CustomsFeeAmount")
        {
            if (!decimal.TryParse(request.Value, out var fee) || fee < 0)
            {
                return Result.Failure(Error.Validation("SiteSetting.InvalidCustomsFee", "Customs fee must be a valid non-negative number."));
            }
        }

        var setting = await _dbContext.SiteSettings.FirstOrDefaultAsync(s => s.Key == request.Key, cancellationToken);
        
        if (setting == null)
        {
            setting = SiteSetting.Create(request.Key, request.Value);
            _dbContext.SiteSettings.Add(setting);
        }
        else
        {
            setting.UpdateValue(request.Value);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
