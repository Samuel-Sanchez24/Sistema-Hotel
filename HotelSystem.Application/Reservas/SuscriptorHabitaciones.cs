using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Interfaces;

namespace HotelSystem.Application.Reservas;

/// <summary>
/// Conecta los observadores registrados (los paneles) con una habitación antes de cambiar
/// su estado. Los servicios lo llaman justo antes de un check-in, un check-out o una
/// actualización de housekeeping; a partir de ahí es la propia <see cref="Habitacion"/>
/// la que notifica.
///
/// Para agregar otro observador basta con registrarlo en DependencyInjection.cs:
/// ningún servicio cambia.
/// </summary>
public sealed class SuscriptorHabitaciones(IEnumerable<IObservadorHabitacion> observadores)
{
    private readonly IObservadorHabitacion[] _observadores = observadores.ToArray();

    public void Suscribir(Habitacion habitacion)
    {
        foreach (var observador in _observadores)
        {
            habitacion.Suscribir(observador);
        }
    }
}
