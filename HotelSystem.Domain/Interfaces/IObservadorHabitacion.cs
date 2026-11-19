using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Domain.Interfaces;

/// <summary>
/// Patrón Observer: observador de los cambios de estado de una habitación.
/// El sujeto es <see cref="Habitacion"/>; los observadores concretos son
/// <c>PanelRecepcion</c> (HU-05) y <c>PanelHousekeeping</c> (HU-11), en la capa Application.
/// </summary>
public interface IObservadorHabitacion
{
    /// <summary>Se llama cada vez que la habitación cambia de estado.</summary>
    void Actualizar(Habitacion habitacion, EstadoHabitacion estadoAnterior);
}
