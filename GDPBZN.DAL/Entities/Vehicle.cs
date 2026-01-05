using System.ComponentModel.DataAnnotations;

namespace GDPBZN.DAL.Entities;

public class Vehicle
{
    public int Id { get; set; }

    public int FireStationId { get; set; }
    public FireStation FireStation { get; set; } = default!;

    [MaxLength(50)]
    public string CallSign { get; set; } = default!;

    [MaxLength(50)]
    public string PlateNumber { get; set; } = default!;

    [MaxLength(100)]
    public string Type { get; set; } = "FireTruck";
    
    public double? LastLat { get; set; }
    public double? LastLng { get; set; }
    public DateTime? LastLocationAtUtc { get; set; }

    public ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
    public ICollection<IncidentUnit> IncidentUnits { get; set; } = new List<IncidentUnit>();
}