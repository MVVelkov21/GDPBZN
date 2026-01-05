using GDPBZN.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace GDPBZN.DAL;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<FireStation> FireStations => Set<FireStation>();
    public DbSet<Vehicle> Vehicles => Set<Vehicle>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Leave> Leaves => Set<Leave>();
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<ShiftAssignment> ShiftAssignments => Set<ShiftAssignment>();

    public DbSet<Incident> Incidents => Set<Incident>();
    public DbSet<IncidentUnit> IncidentUnits => Set<IncidentUnit>();
    public DbSet<IncidentUnitMember> IncidentUnitMembers => Set<IncidentUnitMember>();

    public DbSet<IncidentTask> IncidentTasks => Set<IncidentTask>();
    public DbSet<IncidentTaskAssignment> IncidentTaskAssignments => Set<IncidentTaskAssignment>();

    public DbSet<ResourceRequest> ResourceRequests => Set<ResourceRequest>();

    public DbSet<HazardousSubstance> HazardousSubstances => Set<HazardousSubstance>();
    public DbSet<IncidentHazard> IncidentHazards => Set<IncidentHazard>();

    public DbSet<IncidentAnnotation> IncidentAnnotations => Set<IncidentAnnotation>();
    public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();

    public DbSet<MediaItem> MediaItems => Set<MediaItem>();
    public DbSet<EmergencySignal> EmergencySignals => Set<EmergencySignal>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);

        b.Entity<FireStation>().HasIndex(x => x.Code).IsUnique();
        b.Entity<Vehicle>().HasIndex(x => x.CallSign).IsUnique();
        b.Entity<Employee>().HasIndex(x => x.Username).IsUnique();
        b.Entity<Employee>().HasIndex(x => x.BadgeNumber).IsUnique();

        b.Entity<ShiftAssignment>()
            .HasIndex(x => new { x.ShiftId, x.EmployeeId })
            .IsUnique();

        b.Entity<IncidentUnit>()
            .HasIndex(x => new { x.IncidentId, x.VehicleId })
            .IsUnique();

        b.Entity<IncidentUnitMember>()
            .HasIndex(x => new { x.IncidentUnitId, x.EmployeeId })
            .IsUnique();
        
        b.Entity<MessageTemplate>().HasData(
            new MessageTemplate { Id = 1, Title = "Пристигаме на място", Text = "Екипът пристигна на място. Започваме оценка на обстановката." },
            new MessageTemplate { Id = 2, Title = "Нуждаем се от вода", Text = "Нуждаем се от допълнителна цистерна с вода на локацията." },
            new MessageTemplate { Id = 3, Title = "Опасни вещества", Text = "Идентифицирани са опасни вещества. Изисква се изолация на периметър." },
            new MessageTemplate { Id = 4, Title = "Нуждаем се от медицинска помощ", Text = "Нуждаем се от медицински екип на място." }
        );
    }
}
