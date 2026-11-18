using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Application.Usuarios;

/// <summary>Gestión de cuentas (HU-01, RF-12, RN-08) y registro de huéspedes.</summary>
public interface IUsuarioService
{
    IReadOnlyList<EmpleadoDto> ListarPersonal();

    /// <summary>Crea la cuenta de un empleado con un rol y devuelve su contraseña temporal.</summary>
    EmpleadoCreadoDto CrearEmpleado(string nombre, string correo, RolEmpleado rol, int administradorId);

    /// <summary>Activa o desactiva una cuenta. Al reactivarla conserva su rol (HU-01 Esc.5).</summary>
    EmpleadoDto CambiarEstado(int empleadoId, bool activo, int administradorId);

    /// <summary>Invierte el estado de la cuenta: activa pasa a desactivada y viceversa.</summary>
    EmpleadoDto AlternarEstado(int empleadoId, int administradorId);

    /// <summary>Reemplaza el rol del empleado: pierde los permisos del rol anterior (RN-08).</summary>
    EmpleadoDto CambiarRol(int empleadoId, RolEmpleado rol, int administradorId);

    /// <summary>Registra un huésped. Exige mayoría de edad (RN-10).</summary>
    HuespedDto RegistrarHuesped(DatosNuevoUsuario datos);
}
