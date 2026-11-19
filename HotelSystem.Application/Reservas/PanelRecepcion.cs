using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Interfaces;

namespace HotelSystem.Application.Reservas;

/// <summary>Cambio de estado de una habitación tal como lo ve recepción.</summary>
public sealed record NovedadHabitacion(string Numero, EstadoHabitacion EstadoAnterior, EstadoHabitacion EstadoNuevo, DateTime Momento);

/// <summary>
/// Patrón Observer (observador concreto). Recepción necesita ver la disponibilidad en
/// tiempo real (HU-05, RF-07): este panel recibe TODOS los cambios de estado, guarda los
/// más recientes y aumenta su número de versión. La pantalla de recepción consulta esa
/// versión cada pocos segundos y se recarga sola cuando cambia.
///
/// Vive en memoria y se registra una sola instancia para toda la aplicación.
/// </summary>
public sealed class PanelRecepcion : IObservadorHabitacion
{
    private const int MaximoNovedades = 20;

    private readonly object _candado = new();
    private readonly LinkedList<NovedadHabitacion> _novedades = new();
    private long _version;

    /// <summary>Aumenta en uno con cada cambio. Sirve para saber si hay algo nuevo que mostrar.</summary>
    public long Version => Interlocked.Read(ref _version);

    /// <summary>Últimos cambios, del más reciente al más antiguo.</summary>
    public IReadOnlyList<NovedadHabitacion> UltimasNovedades
    {
        get
        {
            lock (_candado)
            {
                return _novedades.ToList();
            }
        }
    }

    public void Actualizar(Habitacion habitacion, EstadoHabitacion estadoAnterior)
    {
        lock (_candado)
        {
            _novedades.AddFirst(new NovedadHabitacion(habitacion.Numero, estadoAnterior, habitacion.Estado, habitacion.UltimaActualizacion));
            while (_novedades.Count > MaximoNovedades)
            {
                _novedades.RemoveLast();
            }
        }

        Interlocked.Increment(ref _version);
    }
}
