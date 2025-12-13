using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GDPBZN.BLL.Services;

public class TokenService
{
    private readonly IConfiguration _cfg;

    public TokenService(IConfiguration cfg) => _cfg = cfg;

    public string CreateToken(int employeeId, string fullName, string role)
    {
        var key = _cfg["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
        var issuer = _cfg["Jwt:Issuer"] ?? "gdpbzn-api";
        var audience = _cfg["Jwt:Audience"] ?? "gdpbzn-app";
        var expiresMin = int.TryParse(_cfg["Jwt:ExpiresMinutes"], out var m) ? m : 720;

        var creds = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            SecurityAlgorithms.HmacSha256
        );

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, employeeId.ToString()),
            new Claim("eid", employeeId.ToString()),
            new Claim("name", fullName),
            new Claim(ClaimTypes.Name, fullName),
            new Claim(ClaimTypes.Role, role),
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMin),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}