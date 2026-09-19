using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace SubastaYa.API.Hubs;

// Sala pública: mirar una subasta no pide token, igual que GET /api/auctions/{id}.
[AllowAnonymous]
public class AuctionHub : Hub
{
    public static string GroupName(Guid auctionId) => $"auction:{auctionId}";

    public Task JoinAuction(Guid auctionId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, GroupName(auctionId));

    public Task LeaveAuction(Guid auctionId) =>
        Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(auctionId));
}
