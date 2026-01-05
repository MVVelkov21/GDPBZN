using System.ComponentModel.DataAnnotations;

namespace GDPBZN.DAL.Entities;

public class Employee
{
    public int Id { get; set; }

    public int FireStationId { get; set; }
    public FireStation FireStation { get; set; } = default!;

    [MaxLength(50)]
    public string BadgeNumber { get; set; } = default!;

    [MaxLength(120)]
    public string FullName { get; set; } = default!;

    [MaxLength(80)]
    public string Rank { get; set; } = "Firefighter";

    [MaxLength(120)]
    public string? Phone { get; set; }
    
    [MaxLength(64)]
    public string Username { get; set; } = default!;
    [MaxLength(256)]
    public string PasswordHash { get; set; } = default!;
    public bool IsActive { get; set; } = true;

    public ICollection<Leave> Leaves { get; set; } = new List<Leave>();
    public ICollection<ShiftAssignment> ShiftAssignments { get; set; } = new List<ShiftAssignment>();
    public ICollection<IncidentUnitMember> IncidentUnitMemberships { get; set; } = new List<IncidentUnitMember>();
}