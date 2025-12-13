using System.ComponentModel.DataAnnotations;

namespace GDPBZN.DAL.Entities;

public class ResourceRequest
{
    public int Id { get; set; }

    public int IncidentId { get; set; }
    public Incident Incident { get; set; } = default!;

    [MaxLength(200)]
    public string ResourceName { get; set; } = default!; // напр. "Water tanker", "Fuel"

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public int Quantity { get; set; } = 1;

    public ResourceStatus Status { get; set; } = ResourceStatus.Requested;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}