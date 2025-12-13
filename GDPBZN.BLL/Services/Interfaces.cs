using GDPBZN.BLL.DTOs;

namespace GDPBZN.BLL.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest req, CancellationToken ct = default);
}

public interface IIncidentService
{
    Task<List<IncidentListItem>> GetOpenIncidentsAsync(CancellationToken ct = default);
    Task<IncidentDetails?> GetIncidentAsync(int id, CancellationToken ct = default);
    Task<int> CreateIncidentAsync(CreateIncidentRequest req, CancellationToken ct = default);

    Task<bool> AcknowledgeAsync(AcknowledgeRequest req, CancellationToken ct = default);

    Task<int> CreateTaskAsync(CreateTaskRequest req, CancellationToken ct = default);
    Task<int> SendChatAsync(CreateChatRequest req, CancellationToken ct = default);

    Task<bool> UpdateVehicleLocationAsync(int vehicleId, UpdateVehicleLocationRequest req, CancellationToken ct = default);

    Task<int> CreateEmergencyAsync(CreateEmergencyRequest req, CancellationToken ct = default);

    Task<int> CreateAnnotationAsync(CreateAnnotationRequest req, CancellationToken ct = default);

    Task<int> CreateResourceRequestAsync(CreateResourceRequest req, CancellationToken ct = default);
}