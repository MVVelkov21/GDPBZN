namespace GDPBZN.DAL.Entities;

public class IncidentUnit
{
    public int Id { get; set; }

    public int IncidentId { get; set; }
    public Incident Incident { get; set; } = default!;

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    public bool IsAcknowledged { get; set; } // acknowledged от екипа (може и по-детайлно)

    public ICollection<IncidentUnitMember> Members { get; set; } = new List<IncidentUnitMember>();
}

public class IncidentUnitMember
{
    public int Id { get; set; }

    public int IncidentUnitId { get; set; }
    public IncidentUnit IncidentUnit { get; set; } = default!;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;

    public bool Acknowledged { get; set; }
    public DateTime? AcknowledgedAtUtc { get; set; }
}