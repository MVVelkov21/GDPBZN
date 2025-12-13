using System.ComponentModel.DataAnnotations;

namespace GDPBZN.DAL.Entities;

public class EmergencySignal
{
    public int Id { get; set; }

    public int IncidentId { get; set; }
    public Incident Incident { get; set; } = default!;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;

    public double Lat { get; set; }
    public double Lng { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }

    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;
}