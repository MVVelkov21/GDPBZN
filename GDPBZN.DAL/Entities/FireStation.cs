using System.ComponentModel.DataAnnotations;

namespace GDPBZN.DAL.Entities;

public class FireStation
{
    public int Id { get; set; }

    [MaxLength(32)]
    public string Code { get; set; } = default!; 
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(400)]
    public string Address { get; set; } = default!;

    public double? Lat { get; set; }
    public double? Lng { get; set; }

    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<Employee> Employees { get; set; } = new List<Employee>();
}