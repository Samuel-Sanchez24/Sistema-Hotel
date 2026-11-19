using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Domain.Estados;

/// <summary>Habitación lista para recibir huéspedes. Es el único estado que admite check-in.</summary>
public sealed class EstadoDisponible : EstadoHabitacionBase
{
    public override EstadoHabitacion Codigo => EstadoHabitacion.Disponible;

    public override string Nombre => "Disponible";

    public override void Ocupar(Habitacion habitacion, DateTime ahora) =>
        habitacion.CambiarEstado(EstadoHabitacion.Ocupada, ahora, null);

    public override void EnviarALimpieza(Habitacion habitacion, DateTime ahora) =>
        habitacion.CambiarEstado(EstadoHabitacion.EnLimpieza, ahora, null);

    public override void EnviarAMantenimiento(Habitacion habitacion, string nota, DateTime ahora) =>
        habitacion.CambiarEstado(EstadoHabitacion.Mantenimiento, ahora, nota);
}
