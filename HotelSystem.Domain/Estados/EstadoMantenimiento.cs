using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Domain.Estados;

/// <summary>
/// Habitación fuera de servicio por una incidencia. Al terminar la reparación vuelve
/// a estar disponible o pasa a limpieza. No se puede reservar ni ocupar mientras tanto.
/// </summary>
public sealed class EstadoMantenimiento : EstadoHabitacionBase
{
    public override EstadoHabitacion Codigo => EstadoHabitacion.Mantenimiento;

    public override string Nombre => "En mantenimiento";

    public override void MarcarDisponible(Habitacion habitacion, DateTime ahora) =>
        habitacion.CambiarEstado(EstadoHabitacion.Disponible, ahora, null);

    public override void EnviarALimpieza(Habitacion habitacion, DateTime ahora) =>
        habitacion.CambiarEstado(EstadoHabitacion.EnLimpieza, ahora, null);
}
