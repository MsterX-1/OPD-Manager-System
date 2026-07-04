using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OpdManager.Api.Hubs;

[Authorize]
public class QueueHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
    }
}
