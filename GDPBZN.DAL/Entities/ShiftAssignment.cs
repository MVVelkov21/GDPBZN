namespace GDPBZN.DAL.Entities;

public class ShiftAssignment
{
    public int Id { get; set; }

    public int ShiftId { get; set; }
    public Shift Shift { get; set; } = default!;

    public int EmployeeId { get; set; }
    public Employee Employee { get; set; } = default!;

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = default!;

    public bool IsCommander { get; set; }
}