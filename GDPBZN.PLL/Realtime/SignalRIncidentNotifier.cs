using GDPBZN.BLL.Realtime;
using GDPBZN.PLL.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace GDPBZN.PLL.Realtime;

public class SignalRIncidentNotifier : IIncidentNotifier
{
    private readonly IHubContext<IncidentHub> _hub;

    public SignalRIncidentNotifier(IHubContext<IncidentHub> hub)
        => _hub = hub;

    public Task IncidentCreatedAsync(object payload, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("incident.created", payload, ct);

    public Task UnitAcknowledgedAsync(int incidentId, object payload, CancellationToken ct = default)
        => _hub.Clients.Group(Group(incidentId)).SendAsync("unit.ack", payload, ct);

    public Task TaskCreatedAsync(int incidentId, object payload, CancellationToken ct = default)
        => _hub.Clients.Group(Group(incidentId)).SendAsync("task.created", payload, ct);

    public Task ChatNewAsync(int incidentId, object payload, CancellationToken ct = default)
        => _hub.Clients.Group(Group(incidentId)).SendAsync("chat.new", payload, ct);

    public Task VehicleLocationAsync(object payload, CancellationToken ct = default)
        => _hub.Clients.All.SendAsync("vehicle.location", payload, ct);

    public Task EmergencyNewAsync(int incidentId, object payload, CancellationToken ct = default)
        => _hub.Clients.Group(Group(incidentId)).SendAsync("emergency.new", payload, ct);

    public Task AnnotationAsync(int incidentId, object payload, CancellationToken ct = default)
        => _hub.Clients.Group(Group(incidentId)).SendAsync("map.annotation", payload, ct);

    public Task ResourceRequestedAsync(int incidentId, object payload, CancellationToken ct = default)
        => _hub.Clients.Group(Group(incidentId)).SendAsync("resource.requested", payload, ct);

    private static string Group(int incidentId) => $"incident:{incidentId}";
}