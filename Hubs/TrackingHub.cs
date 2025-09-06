using Microsoft.AspNetCore.SignalR;

namespace ShipmentTrackingSystem.Hubs
{
    
    public class TrackingHub : Hub
    {
        public async Task JoinGroup(string trackingNumber) =>
            await Groups.AddToGroupAsync(Context.ConnectionId, trackingNumber);
    }
}
