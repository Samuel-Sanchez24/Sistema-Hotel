using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Domain.Estados;

/// <summary>
/// Habitación pendiente de limpieza. Housekeeping la marca disponible al terminar,
/// o reporta una incidencia y la envía a mantenimiento (HU-11).
/// </summary>
public sealed class EstadoEnLimpieza : EstadoHabitacionBase
{
    public override EstadoHabitacion Codigo => EstadoHabitacion.EnLimpieza;

    public override string Nombre => "En limpieza";

    public override void MarcarDisponible(Habitacion habitacion, DateTime ahora) =>
        habitacion.CambiarEstado(EstadoHabitacion.Disponible, ahora, null);

    public override void EnviarAMantenimiento(Habitacion habitacion, string nota, DateTime ahora) =>
        habitacion.CambiarEstado(EstadoHabitacion.Mantenimiento, ahora, nota);
}
