using System.Security.Cryptography;
using System.Text;

namespace GDPBZN.BLL.Services;

public class PasswordHasher
{
    // Simple demo hash (за продукция: BCrypt/Argon2)
    public string Hash(string password)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        return Convert.ToBase64String(bytes);
    }

    public bool Verify(string password, string hash)
        => Hash(password) == hash;
}