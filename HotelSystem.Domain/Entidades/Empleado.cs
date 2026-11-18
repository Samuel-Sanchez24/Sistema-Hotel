using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Domain.Entidades;

/// <summary>
/// Personal del hotel. El rol es un atributo y no una subclase: así un empleado tiene
/// exactamente un rol (RN-08) y el administrador lo puede cambiar (HU-01 Esc.4).
/// </summary>
public class Empleado : Persona
{
    private Empleado()
    {
    }

    public Empleado(string nombre, string correo, string usuario, string claveHash, RolEmpleado rol, DateTime fechaRegistro, DateOnly? fechaNacimiento = null)
        : base(nombre, correo, usuario, claveHash, fechaNacimiento, fechaRegistro)
    {
        Rol = rol;
    }

    public RolEmpleado Rol { get; private set; }

    public override string RolNombre => Rol.ToString();

    /// <summary>Reemplaza el rol. Al ser un único atributo, los permisos anteriores se pierden (RN-08).</summary>
    public void CambiarRol(RolEmpleado nuevoRol) => Rol = nuevoRol;
}
