using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.Comun;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Application.Usuarios;

/// <summary>
/// Creador concreto del Factory Method: produce un <see cref="Empleado"/> con un único
/// rol (RN-08). Sirve para cualquier rol del personal, porque el rol es un dato.
/// </summary>
public sealed class CreadorEmpleado : CreadorUsuario
{
    public CreadorEmpleado(IPersonaRepository personas, IHasherContrasenas hasher, IReloj reloj, Bitacora bitacora)
        : base(personas, hasher, reloj, bitacora)
    {
    }

    protected override void ValidarDatosPropios(DatosNuevoUsuario datos)
    {
        if (datos.Rol is null)
            throw new ReglaNegocioException("Debe asignar un rol al empleado (RN-08).", "RN-08");
    }

    protected override Persona CrearUsuario(DatosNuevoUsuario datos, string usuario, string claveHash) =>
        new Empleado(datos.Nombre, datos.Correo, usuario, claveHash, datos.Rol!.Value, Reloj.Ahora, datos.FechaNacimiento);
}
