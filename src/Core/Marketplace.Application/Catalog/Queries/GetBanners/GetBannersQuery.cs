using MediatR;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using Marketplace.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Catalog.Queries.GetBanners
{
    public class GetBannersQuery : IRequest<Result<List<BannerDto>>>
    {
        public bool OnlyActive { get; set; }
    }

    public class BannerDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
        public int Position { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class GetBannersQueryHandler : IRequestHandler<GetBannersQuery, Result<List<BannerDto>>>
    {
        private readonly IApplicationDbContext _context;

        public GetBannersQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<List<BannerDto>>> Handle(GetBannersQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Banners.AsQueryable();

            if (request.OnlyActive)
            {
                var now = DateTime.UtcNow;
                query = query.Where(b => b.IsActive 
                                      && (!b.StartDate.HasValue || b.StartDate.Value <= now)
                                      && (!b.EndDate.HasValue || b.EndDate.Value >= now));
            }

            var banners = await query.Select(b => new BannerDto
            {
                Id = b.Id,
                Title = b.Title,
                ImageUrl = b.ImageUrl,
                LinkUrl = b.LinkUrl,
                Position = (int)b.Position,
                StartDate = b.StartDate,
                EndDate = b.EndDate,
                IsActive = b.IsActive
            }).ToListAsync(cancellationToken);

            return Result.Success(banners);
        }
    }
}
