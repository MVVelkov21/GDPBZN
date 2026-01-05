using System.ComponentModel.DataAnnotations;

namespace GDPBZN.DAL.Entities;

public class Incident
{
    public int Id { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public IncidentType Type { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.New;

    [MaxLength(400)]
    public string AddressText { get; set; } = default!;
    public double? Lat { get; set; }
    public double? Lng { get; set; }

    [MaxLength(2000)]
    public string? Description { get; set; }

    [MaxLength(80)]
    public string SourceChannel { get; set; } = "112";

    public ICollection<IncidentUnit> Units { get; set; } = new List<IncidentUnit>();
    public ICollection<IncidentTask> Tasks { get; set; } = new List<IncidentTask>();
    public ICollection<ResourceRequest> ResourceRequests { get; set; } = new List<ResourceRequest>();
    public ICollection<IncidentAnnotation> Annotations { get; set; } = new List<IncidentAnnotation>();
    public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    public ICollection<MediaItem> MediaItems { get; set; } = new List<MediaItem>();
    public ICollection<EmergencySignal> EmergencySignals { get; set; } = new List<EmergencySignal>();

    public ICollection<IncidentHazard> IncidentHazards { get; set; } = new List<IncidentHazard>();
}