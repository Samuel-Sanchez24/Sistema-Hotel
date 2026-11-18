namespace HotelSystem.Application.Usuarios;

/// <summary>Inicio de sesión (HU-02).</summary>
public interface IAuthService
{
    /// <summary>
    /// Valida las credenciales. Lanza <c>ReglaNegocioException</c> si son incorrectas (Esc.2)
    /// o si la cuenta está desactivada (Esc.3).
    /// </summary>
    UsuarioAutenticadoDto IniciarSesion(string usuario, string contrasena);
}
