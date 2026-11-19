using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Application.Reservas;

/// <summary>Resumen de un tipo de habitación para el primer paso de la reserva.</summary>
public sealed record TipoHabitacionDto(TipoHabitacion Tipo, decimal PrecioNoche, int Capacidad, int Habitaciones);

/// <summary>Datos que envía el huésped para reservar una habitación (HU-03).</summary>
/// <param name="MayorDeEdadConfirmado">Declaración del formulario. Además se valida la fecha de nacimiento (RN-10).</param>
/// <param name="MedioPago">Respaldo de pago. Sin él la reserva no se confirma (RN-04).</param>
public sealed record SolicitudReservaHabitacion(
    int HuespedId,
    int HabitacionId,
    DateOnly FechaEntrada,
    DateOnly FechaSalida,
    int CantidadHuespedes,
    bool MayorDeEdadConfirmado,
    TipoPago? MedioPago,
    string? DetallePago);

/// <summary>Reserva recién confirmada y resultado del correo de confirmación (HU-04).</summary>
public sealed record ReservaConfirmadaDto(
    int ReservaId,
    string Codigo,
    string NumeroHabitacion,
    TipoHabitacion TipoHabitacion,
    DateOnly FechaEntrada,
    DateOnly FechaSalida,
    int Noches,
    decimal PrecioNoche,
    decimal ValorTotalAlojamiento,
    string CorreoDestino,
    bool CorreoEnviado);

/// <summary>
/// Fila de "Mis reservas": sirve tanto para una reserva de habitación como para una de tour.
/// Los campos que no aplican al tipo quedan nulos.
/// </summary>
/// <param name="CancelacionTardia">RN-09: cancelar ahora genera cargo.</param>
/// <param name="CargoCancelacion">Cargo que se aplicaría (o que se aplicó, si ya está cancelada).</param>
/// <param name="MotivoNoCancelable">Explicación para el huésped cuando no se puede cancelar.</param>
public sealed record ReservaHuespedDto(
    string Codigo,
    bool EsTour,
    EstadoReserva Estado,
    TipoHabitacion? TipoHabitacion,
    string? NumeroHabitacion,
    DateOnly? FechaEntrada,
    DateOnly? FechaSalida,
    int? Noches,
    decimal? PrecioNoche,
    string? NombreTour,
    DateOnly? FechaTour,
    int? Personas,
    decimal? PrecioPorPersona,
    decimal Total,
    bool Cancelable,
    bool CancelacionTardia,
    decimal CargoCancelacion,
    string? MotivoNoCancelable);

public sealed record CancelacionDto(string Codigo, bool Tardia, decimal Cargo);

/// <summary>Llegada o salida del día en recepción (HU-06).</summary>
public sealed record MovimientoDto(
    int ReservaId,
    string Codigo,
    string Huesped,
    int HabitacionId,
    string NumeroHabitacion,
    EstadoHabitacion EstadoHabitacion,
    DateOnly FechaEntrada,
    DateOnly FechaSalida,
    int Noches,
    EstadoReserva Estado)
{
    public static MovimientoDto Desde(ReservaHabitacion reserva) => new(
        reserva.Id,
        reserva.Codigo,
        reserva.Huesped.Nombre,
        reserva.HabitacionId,
        reserva.Habitacion.Numero,
        reserva.Habitacion.Estado,
        reserva.FechaEntrada,
        reserva.FechaSalida,
        reserva.Noches,
        reserva.Estado);
}
