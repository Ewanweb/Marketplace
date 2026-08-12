using MediatR;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using Marketplace.Domain.Entities;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Catalog.Commands.UpdateBanner
{
    public class UpdateBannerCommand : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string ImageUrl { get; set; } = string.Empty;
        public string LinkUrl { get; set; } = string.Empty;
        public BannerPosition Position { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class UpdateBannerCommandHandler : IRequestHandler<UpdateBannerCommand, Result<Unit>>
    {
        private readonly IApplicationDbContext _context;

        public UpdateBannerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Unit>> Handle(UpdateBannerCommand request, CancellationToken cancellationToken)
        {
            var banner = await _context.Banners.FindAsync(new object[] { request.Id }, cancellationToken);

            if (banner == null)
            {
                return Result.Failure<Unit>(new Error("Banner.NotFound", "Banner not found."));
            }

            banner.Update(
                request.Title,
                request.ImageUrl,
                request.LinkUrl,
                request.Position,
                request.StartDate,
                request.EndDate
            );

            if (request.IsActive)
                banner.Activate();
            else
                banner.Deactivate();

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(Unit.Value);
        }
    }
}
