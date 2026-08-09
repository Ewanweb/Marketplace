using Marketplace.API.Hubs;
using Marketplace.Application.Common.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace Marketplace.API.Services;

public class SignalRMarketplaceEventPublisher : IMarketplaceEventPublisher
{
    private readonly IHubContext<CatalogHub> _hubContext;

    public SignalRMarketplaceEventPublisher(IHubContext<CatalogHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task PublishProductAddedEvent(Guid productId, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("ProductAdded", new { ProductId = productId }, cancellationToken);
    }

    public async Task PublishOrderUpdatedEvent(CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.All.SendAsync("OrderUpdated", new { Timestamp = DateTime.UtcNow }, cancellationToken);
    }
}
