using HotelSystem.Application.Comun;
using HotelSystem.Domain.Entidades;

namespace HotelSystem.Application.Reservas;

/// <summary>Resultado de evaluar la cancelación de una reserva en un momento dado.</summary>
/// <param name="EsTardia">La cancelación ocurre después del límite sin cargo.</param>
/// <param name="Cargo">Valor a cobrar. Cero si no es tardía.</param>
/// <param name="LimiteSinCargo">Último momento en que se puede cancelar gratis.</param>
public sealed record ResultadoCancelacion(bool EsTardia, decimal Cargo, DateTime LimiteSinCargo);

/// <summary>
/// Política que decide el cargo por cancelar una reserva (RN-09). Está detrás de una
/// interfaz para que el hotel pueda cambiar la regla (otro plazo, otro cargo, tarifas no
/// reembolsables) creando una clase nueva, sin modificar <c>ReservaService</c>.
///
/// Es una mejora de diseño sobre el patrón Strategy: el uso que cuenta como patrón del
/// proyecto es el de la exportación de reportes (<c>IExportadorReporte</c>).
/// </summary>
public interface IPoliticaCancelacion
{
    ResultadoCancelacion Evaluar(ReservaHabitacion reserva, DateTime ahora);
}

/// <summary>
/// RN-09: cancelar es gratis hasta 48 horas antes del check-in; después se cobra una noche.
/// El plazo y la hora de check-in vienen de <see cref="OpcionesHotel"/>.
/// </summary>
public sealed class PoliticaCancelacion48Horas(OpcionesHotel opciones) : IPoliticaCancelacion
{
    public ResultadoCancelacion Evaluar(ReservaHabitacion reserva, DateTime ahora)
    {
        var checkIn = reserva.FechaEntrada.ToDateTime(opciones.HoraCheckIn);
        var limite = checkIn.AddHours(-opciones.HorasCancelacionSinCargo);
        var esTardia = ahora > limite;

        return new ResultadoCancelacion(esTardia, esTardia ? reserva.PrecioNoche : 0m, limite);
    }
}
