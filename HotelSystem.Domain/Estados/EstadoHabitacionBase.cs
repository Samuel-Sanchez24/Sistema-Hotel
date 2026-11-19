using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Domain.Estados;

/// <summary>
/// Base de los estados concretos: por defecto toda transición se rechaza (RN-02).
/// Cada estado sobrescribe únicamente las transiciones que sí permite.
/// </summary>
public abstract class EstadoHabitacionBase : IEstadoHabitacion
{
    public abstract EstadoHabitacion Codigo { get; }

    public abstract string Nombre { get; }

    public virtual void Ocupar(Habitacion habitacion, DateTime ahora) =>
        throw Rechazar(habitacion, "registrar el check-in");

    public virtual void Liberar(Habitacion habitacion, DateTime ahora) =>
        throw Rechazar(habitacion, "registrar el check-out");

    public virtual void MarcarDisponible(Habitacion habitacion, DateTime ahora) =>
        throw Rechazar(habitacion, "marcarla como disponible");

    public virtual void EnviarALimpieza(Habitacion habitacion, DateTime ahora) =>
        throw Rechazar(habitacion, "enviarla a limpieza");

    public virtual void EnviarAMantenimiento(Habitacion habitacion, string nota, DateTime ahora) =>
        throw Rechazar(habitacion, "enviarla a mantenimiento");

    protected ReglaNegocioException Rechazar(Habitacion habitacion, string accion) =>
        new($"La habitacion {habitacion.Numero} esta {Nombre.ToLowerInvariant()}: no se puede {accion} (RN-02).", "RN-02");
}
