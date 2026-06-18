namespace ControlTaxi.Application.Common.Interfaces;

/// <summary>Hash y verificación de contraseñas. Implementado en Infrastructure (PBKDF2).</summary>
public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string password, string hash);
}
