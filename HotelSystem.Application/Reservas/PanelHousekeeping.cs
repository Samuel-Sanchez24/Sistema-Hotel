using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Interfaces;

namespace HotelSystem.Application.Reservas;

/// <summary>Habitación que entró a limpieza y espera a ser atendida.</summary>
public sealed record TareaPendiente(int HabitacionId, string Numero, DateTime Desde);

/// <summary>
/// Patrón Observer (observador concreto). A housekeeping solo le interesan los cambios que
/// tocan su trabajo: cuando una habitación queda EN LIMPIEZA la agrega a su cola (HU-11),
/// y cuando sale de limpieza o de mantenimiento la retira. Un check-in, por ejemplo, lo ignora.
///
/// Igual que <see cref="PanelRecepcion"/>, expone un número de versión para que las
/// pantallas de housekeeping se actualicen solas.
/// </summary>
public sealed class PanelHousekeeping : IObservadorHabitacion
{
    private readonly object _candado = new();
    private readonly Dictionary<int, TareaPendiente> _cola = new();
    private long _version;

    public long Version => Interlocked.Read(ref _version);

    /// <summary>Habitaciones que entraron a limpieza desde que arrancó la aplicación, de la más antigua a la más nueva.</summary>
    public IReadOnlyList<TareaPendiente> ColaLimpieza
    {
        get
        {
            lock (_candado)
            {
                return _cola.Values.OrderBy(t => t.Desde).ToList();
            }
        }
    }

    public void Actualizar(Habitacion habitacion, EstadoHabitacion estadoAnterior)
    {
        var entra = habitacion.Estado is EstadoHabitacion.EnLimpieza or EstadoHabitacion.Mantenimiento;
        var sale = estadoAnterior is EstadoHabitacion.EnLimpieza or EstadoHabitacion.Mantenimiento;
        if (!entra && !sale) return;

        lock (_candado)
        {
            if (habitacion.Estado == EstadoHabitacion.EnLimpieza)
            {
                _cola[habitacion.Id] = new TareaPendiente(habitacion.Id, habitacion.Numero, habitacion.UltimaActualizacion);
            }
            else
            {
                _cola.Remove(habitacion.Id);
            }
        }

        Interlocked.Increment(ref _version);
    }
}
