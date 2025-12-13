namespace GDPBZN.DAL.Entities;

public class Shift
{
    public int Id { get; set; }

    public int FireStationId { get; set; }
    public FireStation FireStation { get; set; } = default!;

    public DateTime StartsAtUtc { get; set; }
    public DateTime EndsAtUtc { get; set; }

    public string ShiftName { get; set; } = "Day";

    public ICollection<ShiftAssignment> Assignments { get; set; } = new List<ShiftAssignment>();
}