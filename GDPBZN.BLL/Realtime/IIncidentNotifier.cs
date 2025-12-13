namespace GDPBZN.BLL.Realtime;

public interface IIncidentNotifier
{
    Task IncidentCreatedAsync(object payload, CancellationToken ct = default);

    Task UnitAcknowledgedAsync(int incidentId, object payload, CancellationToken ct = default);
    Task TaskCreatedAsync(int incidentId, object payload, CancellationToken ct = default);
    Task ChatNewAsync(int incidentId, object payload, CancellationToken ct = default);

    Task VehicleLocationAsync(object payload, CancellationToken ct = default);

    Task EmergencyNewAsync(int incidentId, object payload, CancellationToken ct = default);
    Task AnnotationAsync(int incidentId, object payload, CancellationToken ct = default);
    Task ResourceRequestedAsync(int incidentId, object payload, CancellationToken ct = default);
}