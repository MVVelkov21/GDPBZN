using GDPBZN.BLL.DTOs;
using GDPBZN.BLL.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GDPBZN.PLL.Controllers;

[ApiController]
[Route("api/vehicles")]
[Authorize]
public class VehiclesController : ControllerBase
{
    private readonly IIncidentService _inc;

    public VehiclesController(IIncidentService inc) => _inc = inc;

    [HttpPost("{vehicleId:int}/location")]
    public async Task<ActionResult> UpdateLocation(int vehicleId, [FromBody] UpdateVehicleLocationRequest req, CancellationToken ct)
        => (await _inc.UpdateVehicleLocationAsync(vehicleId, req, ct)) ? Ok() : NotFound();
}