using HotelSystem.Application.Housekeeping;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Application.Reservas;

/// <summary>
/// Reservas de habitación y recepción: reservar y cancelar (HU-03), confirmación por correo
/// (HU-04) y check-in / check-out (HU-06).
/// </summary>
public interface IReservaService
{
    /// <summary>Tarifa y capacidad de cada tipo de habitación.</summary>
    IReadOnlyList<TipoHabitacionDto> TiposDeHabitacion();

    /// <summary>
    /// Habitaciones del tipo pedido que están libres en TODO el rango de fechas (RN-01),
    /// admiten esa cantidad de huéspedes (RN-11) y no están en mantenimiento.
    /// </summary>
    IReadOnlyList<HabitacionDto> BuscarDisponibles(TipoHabitacion tipo, DateOnly fechaEntrada, DateOnly fechaSalida, int cantidadHuespedes);

    /// <summary>
    /// Confirma la reserva, bloquea las fechas (RN-01), abre la cuenta de la estancia (RN-07)
    /// y envía el correo de confirmación (HU-04). Valida RN-04, RN-10 y RN-11.
    /// </summary>
    ReservaConfirmadaDto ReservarHabitacion(SolicitudReservaHabitacion solicitud);

    /// <summary>Reservas de habitación y de tour del huésped, con lo que pasaría si las cancela ahora.</summary>
    IReadOnlyList<ReservaHuespedDto> ReservasDeHuesped(int huespedId);

    /// <summary>Cancela una reserva de habitación antes del check-in y aplica el cargo de la RN-09.</summary>
    CancelacionDto CancelarReserva(int reservaId, int huespedId);

    /// <summary>Reservas que esperan check-in hoy.</summary>
    IReadOnlyList<MovimientoDto> LlegadasDeHoy();

    /// <summary>Estancias que deben salir hoy (o que ya debían haber salido).</summary>
    IReadOnlyList<MovimientoDto> SalidasDeHoy();

    /// <summary>Registra el check-in: la reserva queda en curso y la habitación ocupada (RN-02).</summary>
    MovimientoDto RegistrarCheckIn(int reservaId, int empleadoId);

    /// <summary>
    /// Registra la salida: la reserva queda finalizada y la habitación en limpieza (RN-02).
    /// La usa la fachada de cierre de cuenta; recepción no la llama directamente.
    /// </summary>
    MovimientoDto RegistrarSalida(int reservaId, int empleadoId);
}
