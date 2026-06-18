using System.Security.Cryptography;
using ControlTaxi.Application.Common.Interfaces;

namespace ControlTaxi.Infrastructure.Security;

/// <summary>
/// Hash de contraseñas con PBKDF2 (SHA-256, 100k iteraciones, salt de 16 bytes).
/// Formato almacenado: {iteraciones}.{saltBase64}.{hashBase64}.
/// Reemplaza el almacenamiento de contraseñas en claro del sistema anterior.
/// </summary>
public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 100_000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public string Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var key = Rfc2898DeriveBytes.Pbkdf2(password, salt, Iterations, Algorithm, KeySize);
        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(key)}";
    }

    public bool Verify(string password, string hash)
    {
        var parts = hash.Split('.', 3);
        if (parts.Length != 3 || !int.TryParse(parts[0], out var iterations))
            return false;

        try
        {
            var salt = Convert.FromBase64String(parts[1]);
            var key = Convert.FromBase64String(parts[2]);
            var attempt = Rfc2898DeriveBytes.Pbkdf2(password, salt, iterations, Algorithm, key.Length);
            return CryptographicOperations.FixedTimeEquals(attempt, key);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
