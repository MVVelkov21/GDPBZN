using System.ComponentModel.DataAnnotations;

namespace GDPBZN.DAL.Entities;

public class MediaItem
{
    public int Id { get; set; }

    public int IncidentId { get; set; }
    public Incident Incident { get; set; } = default!;

    public int? UploadedByEmployeeId { get; set; }
    public Employee? UploadedByEmployee { get; set; }

    public MediaType Type { get; set; } = MediaType.Photo;

    [MaxLength(1000)]
    public string Url { get; set; } = default!;

    [MaxLength(2000)]
    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}