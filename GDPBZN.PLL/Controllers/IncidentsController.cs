using GDPBZN.BLL.DTOs;
using GDPBZN.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GDPBZN.PLL.Controllers;

[ApiController]
[Route("api/incidents")]
[Authorize]
public class IncidentsController : ControllerBase
{
    private readonly IIncidentService _inc;

    public IncidentsController(IIncidentService inc) => _inc = inc;

    [HttpGet]
    public async Task<ActionResult<List<IncidentListItem>>> GetOpen(CancellationToken ct)
        => Ok(await _inc.GetOpenIncidentsAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<IncidentDetails>> Get(int id, CancellationToken ct)
    {
        var res = await _inc.GetIncidentAsync(id, ct);
        return res is null ? NotFound() : Ok(res);
    }

    [HttpPost]
    public async Task<ActionResult<object>> Create([FromBody] CreateIncidentRequest req, CancellationToken ct)
    {
        var id = await _inc.CreateIncidentAsync(req, ct);
        return Ok(new { IncidentId = id });
    }

    [HttpPost("ack")]
    public async Task<ActionResult> Ack([FromBody] AcknowledgeRequest req, CancellationToken ct)
        => (await _inc.AcknowledgeAsync(req, ct)) ? Ok() : NotFound();

    [HttpPost("tasks")]
    public async Task<ActionResult<object>> CreateTask([FromBody] CreateTaskRequest req, CancellationToken ct)
    {
        var id = await _inc.CreateTaskAsync(req, ct);
        return Ok(new { TaskId = id });
    }

    [HttpPost("chat")]
    public async Task<ActionResult<object>> SendChat([FromBody] CreateChatRequest req, CancellationToken ct)
    {
        var id = await _inc.SendChatAsync(req, ct);
        return Ok(new { MessageId = id });
    }

    [HttpPost("emergency")]
    public async Task<ActionResult<object>> Emergency([FromBody] CreateEmergencyRequest req, CancellationToken ct)
    {
        var id = await _inc.CreateEmergencyAsync(req, ct);
        return Ok(new { EmergencyId = id });
    }

    [HttpPost("annotations")]
    public async Task<ActionResult<object>> CreateAnnotation([FromBody] CreateAnnotationRequest req, CancellationToken ct)
    {
        var id = await _inc.CreateAnnotationAsync(req, ct);
        return Ok(new { AnnotationId = id });
    }

    [HttpPost("resources")]
    public async Task<ActionResult<object>> CreateResource([FromBody] CreateResourceRequest req, CancellationToken ct)
    {
        var id = await _inc.CreateResourceRequestAsync(req, ct);
        return Ok(new { ResourceRequestId = id });
    }
}
