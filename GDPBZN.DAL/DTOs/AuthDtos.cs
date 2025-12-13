namespace GDPBZN.BLL.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, int EmployeeId, string FullName, string Role);