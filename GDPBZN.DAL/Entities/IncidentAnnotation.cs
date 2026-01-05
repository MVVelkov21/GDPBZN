using System.ComponentModel.DataAnnotations;

namespace GDPBZN.DAL.Entities;

public class IncidentAnnotation
{
    public int Id { get; set; }

    public int IncidentId { get; set; }
    public Incident Incident { get; set; } = default!;
    
    [MaxLength(40)]
    public string Kind { get; set; } = "note_pin";
    
    [MaxLength(8000)]
    public string GeometryJson { get; set; } = default!;

    [MaxLength(2000)]
    public string? Text { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}