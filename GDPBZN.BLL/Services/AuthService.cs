using GDPBZN.BLL.DTOs;
using GDPBZN.DAL;
using Microsoft.EntityFrameworkCore;

namespace GDPBZN.BLL.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly TokenService _tokenService;
    private readonly PasswordHasher _hasher;

    public AuthService(AppDbContext db, TokenService tokenService, PasswordHasher hasher)
    {
        _db = db;
        _tokenService = tokenService;
        _hasher = hasher;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        var u = await _db.Employees.FirstOrDefaultAsync(x => x.Username == req.Username && x.IsActive, ct);
        if (u is null) return null;

        if (!_hasher.Verify(req.Password, u.PasswordHash)) return null;

        var role = u.Rank;
        var token = _tokenService.CreateToken(u.Id, u.FullName, role);

        return new LoginResponse(token, u.Id, u.FullName, role);
    }
}