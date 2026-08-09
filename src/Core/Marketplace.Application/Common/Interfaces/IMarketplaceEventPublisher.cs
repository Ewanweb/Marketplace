namespace Marketplace.Application.Common.Interfaces;

public interface IMarketplaceEventPublisher
{
    Task PublishProductAddedEvent(Guid productId, CancellationToken cancellationToken = default);
    Task PublishOrderUpdatedEvent(CancellationToken cancellationToken = default);
}
