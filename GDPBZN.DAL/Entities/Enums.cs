namespace GDPBZN.DAL.Entities;

public enum IncidentType
{
    Fire = 1,
    Rescue = 2,
    Hazmat = 3,
    TrafficAccident = 4,
    Flood = 5,
    Other = 99
}

public enum IncidentStatus
{
    New = 1,
    Dispatched = 2,
    OnScene = 3,
    InProgress = 4,
    Resolved = 5,
    Cancelled = 6
}

public enum LeaveType
{
    Vacation = 1,
    Sick = 2,
    BusinessTrip = 3
}

public enum TaskType
{
    Operational = 1,
    Logistic = 2,
    Administrative = 3,
    Other = 99
}

public enum TaskStatus
{
    Open = 1,
    InProgress = 2,
    Done = 3,
    Cancelled = 4
}

public enum ResourceStatus
{
    Requested = 1,
    Approved = 2,
    Dispatched = 3,
    Delivered = 4,
    Cancelled = 5
}

public enum MediaType
{
    Photo = 1,
    Video = 2,
    Document = 3,
    Other = 99
}