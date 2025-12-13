using System.ComponentModel.DataAnnotations;

namespace GDPBZN.DAL.Entities;

public class IncidentTask
{
    public int Id { get; set; }

    public int IncidentId { get; set; }
    public Incident Incident { get; set; } = default!;

    public TaskType Type { get; set; } = TaskType.Operational;
    public TaskStatus Status { get; set; } = TaskStatus.Open;

    [MaxLength(300)]
    public string Title { get; set; } = default!;

    [MaxLength(4000)]
    public string? Details { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<IncidentTaskAssignment> Assignments { get; set; } = new List<IncidentTaskAssignment>();
}

public class IncidentTaskAssignment
{
    public int Id { get; set; }

    public int IncidentTaskId { get; set; }
    public IncidentTask IncidentTask { get; set; } = default!;

    public int? EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public int? VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

    public DateTime? DoneAtUtc { get; set; }
}