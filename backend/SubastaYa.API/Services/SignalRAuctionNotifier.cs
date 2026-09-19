using Microsoft.AspNetCore.SignalR;
using SubastaYa.API.Hubs;
using SubastaYa.Application.Common.Interfaces;

namespace SubastaYa.API.Services;

public class SignalRAuctionNotifier : IAuctionNotifier
{
    private readonly IHubContext<AuctionHub> _hub;

    public SignalRAuctionNotifier(IHubContext<AuctionHub> hub)
    {
        _hub = hub;
    }

    public Task NotifyAsync(AuctionRealtimeEvent realtimeEvent, CancellationToken cancellationToken = default) =>
        _hub.Clients
            .Group(AuctionHub.GroupName(realtimeEvent.AuctionId))
            .SendAsync("auctionEvent", realtimeEvent, cancellationToken);
}
