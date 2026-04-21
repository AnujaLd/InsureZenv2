using System.Security.Cryptography;
using System.Text;

namespace InsureZenv2.src.Authentication;

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
}

public class PasswordHasher : IPasswordHasher
{
    private const int KeySize = 64; // 512 bits
    private const int IterationCount = 10000;
    private const char Delimiter = ';';

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(KeySize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, IterationCount, HashAlgorithmName.SHA256, KeySize);
        return $"{Convert.ToBase64String(salt)}{Delimiter}{Convert.ToBase64String(key)}";
    }

    public bool VerifyPassword(string password, string hash)
    {
        try
        {
            var parts = hash.Split(Delimiter);
            if (parts.Length != 2)
                return false;

            var salt = Convert.FromBase64String(parts[0]);
            var key = Convert.FromBase64String(parts[1]);

            var keyToCheck = Rfc2898DeriveBytes.Pbkdf2(password, salt, IterationCount, HashAlgorithmName.SHA256, KeySize);
            return CryptographicOperations.FixedTimeEquals(key, keyToCheck);
        }
        catch
        {
            return false;
        }
    }
}
