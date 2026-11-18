using HotelSystem.API.Contratos;
using HotelSystem.API.Seguridad;
using HotelSystem.Application.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.API.Controllers;

/// <summary>Inicio de sesion (HU-02) y registro de huespedes.</summary>
[Route("api/auth")]
[AllowAnonymous]
public sealed class AuthController(IAuthService auth, IUsuarioService usuarios, GeneradorTokens tokens) : ApiControllerBase
{
    /// <summary>Valida las credenciales y devuelve el token. 400 si son incorrectas o la cuenta esta desactivada.</summary>
    [HttpPost("login")]
    public ActionResult<LoginRespuesta> Login(LoginSolicitud solicitud)
    {
        var usuario = auth.IniciarSesion(solicitud.Usuario ?? string.Empty, solicitud.Contrasena ?? string.Empty);
        var token = tokens.Emitir(usuario);

        return new LoginRespuesta(
            token.Token,
            token.ExpiraUtc,
            new UsuarioSesion(usuario.Id, usuario.Nombre, usuario.Correo, usuario.Usuario, RolesApi.De(usuario), usuario.NumeroHabitacion));
    }

    /// <summary>Registro de un huesped nuevo (Factory Method: CreadorHuesped). Exige mayoria de edad (RN-10).</summary>
    [HttpPost("registro")]
    public ActionResult<HuespedDto> Registro(RegistroHuespedSolicitud solicitud)
    {
        var huesped = usuarios.RegistrarHuesped(new DatosNuevoUsuario(
            solicitud.Nombre ?? string.Empty,
            solicitud.Correo ?? string.Empty,
            FechaNacimiento: solicitud.FechaNacimiento,
            Telefono: solicitud.Telefono,
            Usuario: solicitud.Usuario,
            Contrasena: solicitud.Contrasena));

        return StatusCode(StatusCodes.Status201Created, huesped);
    }
}
