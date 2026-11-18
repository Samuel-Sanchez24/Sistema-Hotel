using HotelSystem.API.Contratos;
using HotelSystem.API.Infraestructura;
using HotelSystem.API.Seguridad;
using HotelSystem.Application.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.API.Controllers;

/// <summary>Gestion de cuentas del personal (HU-01, RF-12, RN-08). Solo administrador.</summary>
[Route("api/usuarios")]
[Authorize(Roles = RolesApi.Administrador)]
public sealed class UsuariosController(IUsuarioService usuarios) : ApiControllerBase
{
    /// <summary>Personal del hotel. Nunca incluye contrasenas.</summary>
    [HttpGet]
    public IReadOnlyList<EmpleadoDto> Listar() => usuarios.ListarPersonal();

    /// <summary>Crea la cuenta con un unico rol (Factory Method: CreadorEmpleado) y devuelve la contrasena temporal.</summary>
    [HttpPost]
    public ActionResult<UsuarioCreadoRespuesta> Crear(NuevoUsuarioSolicitud solicitud)
    {
        var creado = usuarios.CrearEmpleado(
            TextoRequerido(solicitud.Nombre, "Complete nombre y correo."),
            TextoRequerido(solicitud.Correo, "Complete nombre y correo."),
            LectorValores.Rol(solicitud.Rol),
            UsuarioId);

        return StatusCode(StatusCodes.Status201Created, UsuarioCreadoRespuesta.Desde(creado));
    }

    /// <summary>Reemplaza el rol: el empleado pierde los permisos del anterior (RN-08). Aplica desde su proximo inicio de sesion.</summary>
    [HttpPatch("{id:int}/rol")]
    public EmpleadoDto CambiarRol(int id, CambioRolSolicitud solicitud) =>
        usuarios.CambiarRol(id, LectorValores.Rol(solicitud.Rol), UsuarioId);

    /// <summary>Activa o desactiva la cuenta. Al reactivarla conserva su rol (HU-01 Esc.5).</summary>
    [HttpPatch("{id:int}/estado")]
    public EmpleadoDto CambiarEstado(int id, CambioEstadoUsuarioSolicitud solicitud) =>
        usuarios.CambiarEstado(id, solicitud.Activo, UsuarioId);
}
