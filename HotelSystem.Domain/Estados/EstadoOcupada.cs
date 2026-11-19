using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Domain.Estados;

/// <summary>
/// Habitación con un huésped alojado. Solo sale de este estado con el check-out,
/// y siempre pasa por limpieza antes de volver a estar disponible.
/// </summary>
public sealed class EstadoOcupada : EstadoHabitacionBase
{
    public override EstadoHabitacion Codigo => EstadoHabitacion.Ocupada;

    public override string Nombre => "Ocupada";

    public override void Liberar(Habitacion habitacion, DateTime ahora) =>
        habitacion.CambiarEstado(EstadoHabitacion.EnLimpieza, ahora, null);
}
