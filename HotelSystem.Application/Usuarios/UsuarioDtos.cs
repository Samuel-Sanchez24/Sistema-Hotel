using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Application.Usuarios;

/// <summary>Resultado de un inicio de sesión correcto (HU-02).</summary>
/// <param name="Rol">"Huesped" o el nombre del rol del empleado.</param>
/// <param name="RolPersonal">Rol del empleado; nulo si es un huésped.</param>
/// <param name="NumeroHabitacion">Habitación de la estancia en curso del huésped, si tiene.</param>
public sealed record UsuarioAutenticadoDto(
    int Id,
    string Nombre,
    string Correo,
    string Usuario,
    string Rol,
    bool EsHuesped,
    RolEmpleado? RolPersonal,
    string? NumeroHabitacion);

/// <summary>Cuenta de un empleado. Nunca incluye la contraseña.</summary>
public sealed record EmpleadoDto(int Id, string Nombre, string Correo, string Usuario, RolEmpleado Rol, bool Activo)
{
    public static EmpleadoDto Desde(Empleado empleado) =>
        new(empleado.Id, empleado.Nombre, empleado.Correo, empleado.Usuario, empleado.Rol, empleado.Activo);
}

/// <summary>Empleado recién creado junto con su contraseña temporal, que solo se muestra esta vez.</summary>
public sealed record EmpleadoCreadoDto(EmpleadoDto Empleado, string ContrasenaTemporal);

public sealed record HuespedDto(int Id, string Nombre, string Correo, string Usuario, DateOnly? FechaNacimiento, string? Telefono)
{
    public static HuespedDto Desde(Huesped huesped) =>
        new(huesped.Id, huesped.Nombre, huesped.Correo, huesped.Usuario, huesped.FechaNacimiento, huesped.Telefono);
}

/// <summary>
/// Datos para registrar un usuario. Si no se indican usuario o contraseña, el creador los genera.
/// </summary>
/// <param name="Rol">Obligatorio para empleados (RN-08).</param>
/// <param name="FechaNacimiento">Obligatoria para huéspedes (RN-10).</param>
/// <param name="RegistradoPorId">Empleado que hace el registro, para la bitácora.</param>
public sealed record DatosNuevoUsuario(
    string Nombre,
    string Correo,
    DateOnly? FechaNacimiento = null,
    RolEmpleado? Rol = null,
    string? Telefono = null,
    string? Usuario = null,
    string? Contrasena = null,
    int? RegistradoPorId = null);

/// <summary>Usuario creado por un <see cref="CreadorUsuario"/>, con la contraseña en claro para entregarla una vez.</summary>
public sealed record UsuarioCreado(Persona Persona, string Contrasena, bool ContrasenaTemporal);
