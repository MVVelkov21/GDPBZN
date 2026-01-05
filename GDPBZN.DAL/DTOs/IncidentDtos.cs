using GDPBZN.DAL.Entities;
using TaskStatus = GDPBZN.DAL.Entities.TaskStatus;

namespace GDPBZN.BLL.DTOs;

public record CreateIncidentRequest(
    IncidentType Type,
    string AddressText,
    double? Lat,
    double? Lng,
    string? Description,
    string SourceChannel,
    int FireStationId,
    int? ShiftId,
    List<int>? VehicleIds
);

public record IncidentListItem(
    int Id,
    DateTime CreatedAtUtc,
    IncidentType Type,
    IncidentStatus Status,
    string AddressText,
    double? Lat,
    double? Lng
);

public record IncidentDetails(
    int Id,
    DateTime CreatedAtUtc,
    IncidentType Type,
    IncidentStatus Status,
    string AddressText,
    double? Lat,
    double? Lng,
    string? Description,
    string SourceChannel,
    List<UnitDto> Units,
    List<TaskDto> Tasks,
    List<ResourceDto> Resources,
    List<AnnotationDto> Annotations,
    List<HazardDto> Hazards,
    List<ChatDto> Chat
);

public record UnitDto(int UnitId, int VehicleId, string CallSign, bool IsAcknowledged, List<UnitMemberDto> Members);
public record UnitMemberDto(int EmployeeId, string FullName, bool Acknowledged);

public record CreateTaskRequest(int IncidentId, string Title, string? Details, TaskType Type, List<int>? AssignVehicleIds, List<int>? AssignEmployeeIds);
public record TaskDto(int Id, string Title, TaskType Type, TaskStatus Status, string? Details);

public record CreateChatRequest(int IncidentId, int SenderEmployeeId, string Text, string? AttachmentUrl);
public record ChatDto(int Id, int? SenderEmployeeId, string SenderName, string Text, DateTime SentAtUtc, string? AttachmentUrl);

public record UpdateVehicleLocationRequest(double Lat, double Lng);

public record CreateEmergencyRequest(int IncidentId, int EmployeeId, double Lat, double Lng, string? Notes);
public record EmergencyDto(int Id, int IncidentId, int EmployeeId, string EmployeeName, double Lat, double Lng, string? Notes, DateTime SentAtUtc);

public record CreateAnnotationRequest(int IncidentId, string Kind, string GeometryJson, string? Text);
public record AnnotationDto(int Id, string Kind, string GeometryJson, string? Text, DateTime CreatedAtUtc);

public record CreateResourceRequest(int IncidentId, string ResourceName, int Quantity, string? Notes);
public record ResourceDto(int Id, string ResourceName, int Quantity, ResourceStatus Status, string? Notes);

public record HazardDto(int Id, string UnNumber, string Name, string? Risks, string? ActionPlan);

public record AcknowledgeRequest(int IncidentId, int VehicleId, int EmployeeId);
