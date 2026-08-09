using Microsoft.AspNetCore.SignalR;

namespace Marketplace.API.Hubs;

public class CatalogHub : Hub
{
    // This hub allows anonymous connections since catalogs are public.
    // Methods can be added here if clients need to invoke server logic,
    // but for now, we just use it to broadcast events to clients.
}
