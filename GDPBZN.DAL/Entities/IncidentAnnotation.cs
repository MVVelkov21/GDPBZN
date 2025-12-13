using System.ComponentModel.DataAnnotations;

namespace GDPBZN.DAL.Entities;

public class IncidentAnnotation
{
    public int Id { get; set; }

    public int IncidentId { get; set; }
    public Incident Incident { get; set; } = default!;

    // тип: "fire_front", "wind_dir", "note_pin" и т.н.
    [MaxLength(40)]
    public string Kind { get; set; } = "note_pin";

    // GeoJSON-ish (за простота като string)
    [MaxLength(8000)]
    public string GeometryJson { get; set; } = default!;

    [MaxLength(2000)]
    public string? Text { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}