using GDPBZN.BLL.DTOs;
using GDPBZN.BLL.Realtime;
using GDPBZN.DAL;
using GDPBZN.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using TaskStatus = GDPBZN.DAL.Entities.TaskStatus;

namespace GDPBZN.BLL.Services;

public class IncidentService : IIncidentService
{
    private readonly AppDbContext _db;
    private readonly IIncidentNotifier _notifier;

    public IncidentService(AppDbContext db, IIncidentNotifier notifier)
    {
        _db = db;
        _notifier = notifier;
    }

    public async Task<List<IncidentListItem>> GetOpenIncidentsAsync(CancellationToken ct = default)
    {
        return await _db.Incidents
            .Where(i => i.Status != IncidentStatus.Resolved && i.Status != IncidentStatus.Cancelled)
            .OrderByDescending(i => i.CreatedAtUtc)
            .Select(i => new IncidentListItem(i.Id, i.CreatedAtUtc, i.Type, i.Status, i.AddressText, i.Lat, i.Lng))
            .ToListAsync(ct);
    }

    public async Task<IncidentDetails?> GetIncidentAsync(int id, CancellationToken ct = default)
    {
        var i = await _db.Incidents
            .Include(x => x.Units).ThenInclude(u => u.Vehicle)
            .Include(x => x.Units).ThenInclude(u => u.Members).ThenInclude(m => m.Employee)
            .Include(x => x.Tasks)
            .Include(x => x.ResourceRequests)
            .Include(x => x.Annotations)
            .Include(x => x.IncidentHazards).ThenInclude(h => h.HazardousSubstance)
            .Include(x => x.ChatMessages).ThenInclude(m => m.SenderEmployee)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

        if (i is null) return null;

        var units = i.Units.Select(u => new UnitDto(
            u.Id, u.VehicleId, u.Vehicle.CallSign, u.IsAcknowledged,
            u.Members.Select(m => new UnitMemberDto(m.EmployeeId, m.Employee.FullName, m.Acknowledged)).ToList()
        )).ToList();

        var tasks = i.Tasks
            .OrderByDescending(t => t.CreatedAtUtc)
            .Select(t => new TaskDto(t.Id, t.Title, t.Type, t.Status, t.Details))
            .ToList();

        var res = i.ResourceRequests
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new ResourceDto(r.Id, r.ResourceName, r.Quantity, r.Status, r.Notes))
            .ToList();

        var ann = i.Annotations
            .OrderByDescending(a => a.CreatedAtUtc)
            .Select(a => new AnnotationDto(a.Id, a.Kind, a.GeometryJson, a.Text, a.CreatedAtUtc))
            .ToList();

        var haz = i.IncidentHazards
            .Select(h => h.HazardousSubstance)
            .Select(h => new HazardDto(h.Id, h.UnNumber, h.Name, h.Risks, h.ActionPlan))
            .ToList();

        var chat = i.ChatMessages
            .OrderBy(c => c.SentAtUtc)
            .Select(c => new ChatDto(
                c.Id,
                c.SenderEmployeeId,
                c.SenderEmployee != null ? c.SenderEmployee.FullName : "System",
                c.Text,
                c.SentAtUtc,
                c.AttachmentUrl
            ))
            .ToList();

        return new IncidentDetails(
            i.Id, i.CreatedAtUtc, i.Type, i.Status,
            i.AddressText, i.Lat, i.Lng, i.Description, i.SourceChannel,
            units, tasks, res, ann, haz, chat
        );
    }

    public async Task<int> CreateIncidentAsync(CreateIncidentRequest req, CancellationToken ct = default)
    {
        var incident = new Incident
        {
            Type = req.Type,
            AddressText = req.AddressText,
            Lat = req.Lat,
            Lng = req.Lng,
            Description = req.Description,
            SourceChannel = string.IsNullOrWhiteSpace(req.SourceChannel) ? "112" : req.SourceChannel,
            Status = IncidentStatus.New
        };

        _db.Incidents.Add(incident);
        await _db.SaveChangesAsync(ct);

        // Determine vehicleIds
        var vehicleIds = req.VehicleIds ?? new List<int>();

        if (vehicleIds.Count == 0)
        {
            var now = DateTime.UtcNow;

            var shift = req.ShiftId.HasValue
                ? await _db.Shifts.Include(s => s.Assignments).FirstOrDefaultAsync(s => s.Id == req.ShiftId.Value, ct)
                : await _db.Shifts.Include(s => s.Assignments)
                    .Where(s => s.FireStationId == req.FireStationId && s.StartsAtUtc <= now && s.EndsAtUtc >= now)
                    .OrderByDescending(s => s.StartsAtUtc)
                    .FirstOrDefaultAsync(ct);

            if (shift != null)
            {
                var employeeIds = shift.Assignments.Select(a => a.EmployeeId).Distinct().ToList();

                var absent = await _db.Leaves
                    .Where(l => employeeIds.Contains(l.EmployeeId) && l.FromUtc <= now && l.ToUtc >= now)
                    .Select(l => l.EmployeeId)
                    .ToListAsync(ct);

                var activeAssignments = shift.Assignments
                    .Where(a => !absent.Contains(a.EmployeeId))
                    .ToList();

                vehicleIds = activeAssignments.Select(a => a.VehicleId).Distinct().ToList();
            }
        }

        // Add units + members
        foreach (var vid in vehicleIds.Distinct())
        {
            var unit = new IncidentUnit
            {
                IncidentId = incident.Id,
                VehicleId = vid
            };
            _db.IncidentUnits.Add(unit);
            await _db.SaveChangesAsync(ct);

            var now = DateTime.UtcNow;
            var shiftForMembers = req.ShiftId.HasValue
                ? await _db.Shifts.FirstOrDefaultAsync(s => s.Id == req.ShiftId.Value, ct)
                : await _db.Shifts
                    .Where(s => s.FireStationId == req.FireStationId && s.StartsAtUtc <= now && s.EndsAtUtc >= now)
                    .OrderByDescending(s => s.StartsAtUtc)
                    .FirstOrDefaultAsync(ct);

            if (shiftForMembers != null)
            {
                var ass = await _db.ShiftAssignments
                    .Where(a => a.ShiftId == shiftForMembers.Id && a.VehicleId == vid)
                    .ToListAsync(ct);

                foreach (var a in ass)
                {
                    _db.IncidentUnitMembers.Add(new IncidentUnitMember
                    {
                        IncidentUnitId = unit.Id,
                        EmployeeId = a.EmployeeId
                    });
                }
            }
        }

        incident.Status = IncidentStatus.Dispatched;
        await _db.SaveChangesAsync(ct);

        await _notifier.IncidentCreatedAsync(new
        {
            incident.Id,
            incident.CreatedAtUtc,
            incident.Type,
            incident.Status,
            incident.AddressText,
            incident.Lat,
            incident.Lng
        }, ct);

        return incident.Id;
    }

    public async Task<bool> AcknowledgeAsync(AcknowledgeRequest req, CancellationToken ct = default)
    {
        var unit = await _db.IncidentUnits
            .Include(u => u.Members)
            .FirstOrDefaultAsync(u => u.IncidentId == req.IncidentId && u.VehicleId == req.VehicleId, ct);

        if (unit is null) return false;

        var member = unit.Members.FirstOrDefault(m => m.EmployeeId == req.EmployeeId);
        if (member is null) return false;

        member.Acknowledged = true;
        member.AcknowledgedAtUtc = DateTime.UtcNow;

        unit.IsAcknowledged = unit.Members.All(m => m.Acknowledged);

        await _db.SaveChangesAsync(ct);

        await _notifier.UnitAcknowledgedAsync(req.IncidentId, new
        {
            req.IncidentId,
            req.VehicleId,
            req.EmployeeId
        }, ct);

        return true;
    }

    public async Task<int> CreateTaskAsync(CreateTaskRequest req, CancellationToken ct = default)
    {
        var task = new IncidentTask
        {
            IncidentId = req.IncidentId,
            Title = req.Title,
            Details = req.Details,
            Type = req.Type,
            Status = TaskStatus.Open
        };

        _db.IncidentTasks.Add(task);
        await _db.SaveChangesAsync(ct);

        var assigns = new List<IncidentTaskAssignment>();

        if (req.AssignVehicleIds != null)
            assigns.AddRange(req.AssignVehicleIds.Distinct()
                .Select(vid => new IncidentTaskAssignment { IncidentTaskId = task.Id, VehicleId = vid }));

        if (req.AssignEmployeeIds != null)
            assigns.AddRange(req.AssignEmployeeIds.Distinct()
                .Select(eid => new IncidentTaskAssignment { IncidentTaskId = task.Id, EmployeeId = eid }));

        if (assigns.Count > 0)
        {
            _db.IncidentTaskAssignments.AddRange(assigns);
            await _db.SaveChangesAsync(ct);
        }

        await _notifier.TaskCreatedAsync(req.IncidentId, new
        {
            task.Id,
            task.Title,
            task.Type,
            task.Status
        }, ct);

        return task.Id;
    }

    public async Task<int> SendChatAsync(CreateChatRequest req, CancellationToken ct = default)
    {
        var msg = new ChatMessage
        {
            IncidentId = req.IncidentId,
            SenderEmployeeId = req.SenderEmployeeId,
            Text = req.Text,
            AttachmentUrl = req.AttachmentUrl
        };

        _db.ChatMessages.Add(msg);
        await _db.SaveChangesAsync(ct);

        var sender = await _db.Employees
            .Where(e => e.Id == req.SenderEmployeeId)
            .Select(e => e.FullName)
            .FirstOrDefaultAsync(ct);

        await _notifier.ChatNewAsync(req.IncidentId, new
        {
            msg.Id,
            msg.IncidentId,
            msg.SenderEmployeeId,
            SenderName = sender ?? "Unknown",
            msg.Text,
            msg.SentAtUtc,
            msg.AttachmentUrl
        }, ct);

        return msg.Id;
    }

    public async Task<bool> UpdateVehicleLocationAsync(int vehicleId, UpdateVehicleLocationRequest req, CancellationToken ct = default)
    {
        var v = await _db.Vehicles.FirstOrDefaultAsync(x => x.Id == vehicleId, ct);
        if (v is null) return false;

        v.LastLat = req.Lat;
        v.LastLng = req.Lng;
        v.LastLocationAtUtc = DateTime.UtcNow;

        await _db.SaveChangesAsync(ct);

        await _notifier.VehicleLocationAsync(new
        {
            VehicleId = v.Id,
            v.LastLat,
            v.LastLng,
            v.LastLocationAtUtc
        }, ct);

        return true;
    }

    public async Task<int> CreateEmergencyAsync(CreateEmergencyRequest req, CancellationToken ct = default)
    {
        var s = new EmergencySignal
        {
            IncidentId = req.IncidentId,
            EmployeeId = req.EmployeeId,
            Lat = req.Lat,
            Lng = req.Lng,
            Notes = req.Notes
        };

        _db.EmergencySignals.Add(s);
        await _db.SaveChangesAsync(ct);

        var name = await _db.Employees
            .Where(e => e.Id == req.EmployeeId)
            .Select(e => e.FullName)
            .FirstOrDefaultAsync(ct);

        await _notifier.EmergencyNewAsync(req.IncidentId, new
        {
            s.Id,
            s.IncidentId,
            s.EmployeeId,
            EmployeeName = name ?? "Unknown",
            s.Lat,
            s.Lng,
            s.Notes,
            s.SentAtUtc
        }, ct);

        return s.Id;
    }

    public async Task<int> CreateAnnotationAsync(CreateAnnotationRequest req, CancellationToken ct = default)
    {
        var a = new IncidentAnnotation
        {
            IncidentId = req.IncidentId,
            Kind = req.Kind,
            GeometryJson = req.GeometryJson,
            Text = req.Text
        };

        _db.IncidentAnnotations.Add(a);
        await _db.SaveChangesAsync(ct);

        await _notifier.AnnotationAsync(req.IncidentId, new
        {
            a.Id,
            a.Kind,
            a.GeometryJson,
            a.Text,
            a.CreatedAtUtc
        }, ct);

        return a.Id;
    }

    public async Task<int> CreateResourceRequestAsync(CreateResourceRequest req, CancellationToken ct = default)
    {
        var r = new ResourceRequest
        {
            IncidentId = req.IncidentId,
            ResourceName = req.ResourceName,
            Quantity = req.Quantity,
            Notes = req.Notes,
            Status = ResourceStatus.Requested
        };

        _db.ResourceRequests.Add(r);
        await _db.SaveChangesAsync(ct);

        await _notifier.ResourceRequestedAsync(req.IncidentId, new
        {
            r.Id,
            r.ResourceName,
            r.Quantity,
            r.Status,
            r.Notes
        }, ct);

        return r.Id;
    }
}
