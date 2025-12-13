namespace GDPBZN.DAL.Entities;

public class Leave
{
    public int Id { get; set; }

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;

    public LeaveType Type { get; set; }

    public DateTime FromUtc { get; set; }
    public DateTime ToUtc { get; set; }

    public string? Notes { get; set; }
}