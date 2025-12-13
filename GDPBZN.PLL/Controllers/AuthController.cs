using GDPBZN.BLL.DTOs;
using GDPBZN.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace GDPBZN.PLL.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest req, CancellationToken ct)
    {
        var res = await _auth.LoginAsync(req, ct);
        if (res is null) return Unauthorized();
        return Ok(res);
    }
}