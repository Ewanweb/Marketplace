using MediatR;
using Marketplace.Application.Common.Interfaces;
using Marketplace.Shared.Results;
using System.Threading;
using System.Threading.Tasks;

namespace Marketplace.Application.Catalog.Commands.DeleteBanner
{
    public class DeleteBannerCommand : IRequest<Result<Unit>>
    {
        public Guid Id { get; set; }
    }

    public class DeleteBannerCommandHandler : IRequestHandler<DeleteBannerCommand, Result<Unit>>
    {
        private readonly IApplicationDbContext _context;

        public DeleteBannerCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Result<Unit>> Handle(DeleteBannerCommand request, CancellationToken cancellationToken)
        {
            var banner = await _context.Banners.FindAsync(new object[] { request.Id }, cancellationToken);

            if (banner == null)
            {
                return Result.Failure<Unit>(new Error("Banner.NotFound", "Banner not found."));
            }

            _context.Banners.Remove(banner);
            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success(Unit.Value);
        }
    }
}
