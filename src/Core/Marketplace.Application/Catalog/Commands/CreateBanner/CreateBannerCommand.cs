using MediatR;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using Marketplace.Domain.Entities;

namespace Marketplace.Application.Catalog.Commands.CreateBanner
{
    public class CreateBannerCommand : IRequest<Result<Guid>>
    {
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
        public BannerPosition Position { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class CreateBannerCommandHandler : IRequestHandler<CreateBannerCommand, Result<Guid>>
    {
        private readonly IApplicationDbContext _context;

        public CreateBannerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Guid>> Handle(CreateBannerCommand request, CancellationToken cancellationToken)
        {
            var banner = Banner.Create(
                request.Title,
                request.ImageUrl,
                request.LinkUrl,
                request.Position,
                request.StartDate,
                request.EndDate
            );

            _context.Banners.Add(banner);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(banner.Id);
        }
    }
}
