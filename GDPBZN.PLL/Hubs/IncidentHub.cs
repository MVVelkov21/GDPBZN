using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace GDPBZN.PLL.Hubs;

[Authorize]
public class IncidentHub : Hub
{
    public Task JoinIncident(int incidentId)
        => Groups.AddToGroupAsync(Context.ConnectionId, $"incident:{incidentId}");

    public Task LeaveIncident(int incidentId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, $"incident:{incidentId}");
}