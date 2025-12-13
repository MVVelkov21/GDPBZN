using System.ComponentModel.DataAnnotations;

namespace GDPBZN.DAL.Entities;

public class HazardousSubstance
{
    public int Id { get; set; }

    [MaxLength(80)]
    public string UnNumber { get; set; } = default!; // UN 1203 etc.

    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(4000)]
    public string? Risks { get; set; }

    [MaxLength(4000)]
    public string? ActionPlan { get; set; }
}

public class IncidentHazard
{
    public int Id { get; set; }

    public int IncidentId { get; set; }
    public Incident Incident { get; set; } = default!;

    public int HazardousSubstanceId { get; set; }
    public HazardousSubstance HazardousSubstance { get; set; } = default!;
}