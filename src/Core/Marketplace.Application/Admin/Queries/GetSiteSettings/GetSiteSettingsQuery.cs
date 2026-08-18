using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Marketplace.Application.Admin.Queries.GetSiteSettings;

public sealed record GetSiteSettingsQuery() : IRequest<Result<Dictionary<string, string>>>;

public sealed class GetSiteSettingsQueryHandler : IRequestHandler<GetSiteSettingsQuery, Result<Dictionary<string, string>>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSiteSettingsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<Dictionary<string, string>>> Handle(GetSiteSettingsQuery request, CancellationToken cancellationToken)
    {
        var settings = await _dbContext.SiteSettings
            .ToDictionaryAsync(s => s.Key, s => s.Value, cancellationToken);
            
        return Result.Success(settings);
    }
}
