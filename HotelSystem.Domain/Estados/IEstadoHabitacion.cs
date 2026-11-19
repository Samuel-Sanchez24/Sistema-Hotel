using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Domain.Estados;

/// <summary>
/// Patrón State (RN-02). Cada estado concreto decide qué transiciones permite y rechaza
/// las demás con una <c>ReglaNegocioException</c>. <see cref="Habitacion"/> es el contexto:
/// delega aquí todas sus operaciones de cambio de estado.
///
/// Transiciones válidas:
///   Disponible    -> Ocupada (check-in), EnLimpieza, Mantenimiento
///   Ocupada       -> EnLimpieza (check-out)
///   EnLimpieza    -> Disponible, Mantenimiento (incidencia)
///   Mantenimiento -> Disponible, EnLimpieza
/// </summary>
public interface IEstadoHabitacion
{
    /// <summary>Valor del enum que se guarda en la base de datos.</summary>
    EstadoHabitacion Codigo { get; }

    /// <summary>Nombre para mostrar en mensajes.</summary>
    string Nombre { get; }

    /// <summary>Check-in: la habitación recibe a un huésped.</summary>
    void Ocupar(Habitacion habitacion, DateTime ahora);

    /// <summary>Check-out: el huésped sale y la habitación queda pendiente de limpieza.</summary>
    void Liberar(Habitacion habitacion, DateTime ahora);

    /// <summary>Housekeeping terminó la limpieza o la reparación.</summary>
    void MarcarDisponible(Habitacion habitacion, DateTime ahora);

    void EnviarALimpieza(Habitacion habitacion, DateTime ahora);

    /// <summary>Se reporta una incidencia (HU-11) y la habitación sale de servicio.</summary>
    void EnviarAMantenimiento(Habitacion habitacion, string nota, DateTime ahora);
}
