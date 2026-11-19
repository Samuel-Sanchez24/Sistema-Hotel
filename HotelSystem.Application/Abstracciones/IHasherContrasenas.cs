namespace HotelSystem.Application.Abstracciones;

/// <summary>Convierte contraseñas en un hash irreversible y las verifica al iniciar sesión.</summary>
public interface IHasherContrasenas
{
    string Hash(string contrasena);

    bool Verificar(string contrasena, string hash);
}
