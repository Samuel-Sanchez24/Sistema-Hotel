using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.Comun;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Application.Usuarios;

/// <summary>
/// Creador concreto del Factory Method: produce un <see cref="Huesped"/> y exige que sea
/// mayor de edad, porque será titular de reservas (RN-10).
/// </summary>
public sealed class CreadorHuesped : CreadorUsuario
{
    public CreadorHuesped(IPersonaRepository personas, IHasherContrasenas hasher, IReloj reloj, Bitacora bitacora)
        : base(personas, hasher, reloj, bitacora)
    {
    }

    protected override void ValidarDatosPropios(DatosNuevoUsuario datos)
    {
        if (datos.FechaNacimiento is not { } nacimiento)
            throw new ReglaNegocioException("La fecha de nacimiento es obligatoria para registrar un huesped (RN-10).", "RN-10");
        if (nacimiento.AddYears(18) > Reloj.Hoy)
            throw new ReglaNegocioException("El titular debe ser mayor de 18 anos (RN-10).", "RN-10");
    }

    protected override Persona CrearUsuario(DatosNuevoUsuario datos, string usuario, string claveHash) =>
        new Huesped(datos.Nombre, datos.Correo, usuario, claveHash, datos.FechaNacimiento, Reloj.Ahora, datos.Telefono);
}
