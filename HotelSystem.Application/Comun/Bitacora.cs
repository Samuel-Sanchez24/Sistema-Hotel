using HotelSystem.Application.Abstracciones;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Application.Comun;

/// <summary>
/// Registra en la bitácora las operaciones del personal. El registro se guarda junto con
/// el resto de cambios de la operación, cuando el servicio llama a GuardarCambios().
/// </summary>
public sealed class Bitacora(IRegistroOperacionRepository operaciones, IReloj reloj)
{
    /// <summary>No registra nada si la operación no la hizo un empleado identificado.</summary>
    public void Registrar(int? empleadoId, TipoOperacion tipo, string detalle)
    {
        if (empleadoId is null or <= 0) return;
        operaciones.Agregar(new RegistroOperacion(empleadoId.Value, tipo, detalle, reloj.Ahora));
    }
}
