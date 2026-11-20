using HotelSystem.Application.ServiciosAdicionales;
using HotelSystem.Application.Tours;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Application.CierreCuenta;

/// <summary>
/// Cuenta consolidada de una estancia (HU-12, RF-06, RN-07): alojamiento, consumos de
/// servicios adicionales y tours, con el monto total. Sirve para la vista previa, para la
/// prefactura generada y para el resumen del cierre.
/// </summary>
/// <param name="MontoConsolidado">Alojamiento + servicios + tours.</param>
/// <param name="IdPrefactura">Número del documento, cuando la prefactura ya se generó o la cuenta se cerró.</param>
/// <param name="MetodoPago">Medio con el que se pagó. Nulo mientras la cuenta está abierta.</param>
public sealed record PrefacturaDto(
    int CuentaId,
    int ReservaId,
    string CodigoReserva,
    int HabitacionId,
    string NumeroHabitacion,
    TipoHabitacion TipoHabitacion,
    string NombreHuesped,
    DateOnly FechaEntrada,
    DateOnly FechaSalida,
    int Noches,
    decimal PrecioNoche,
    decimal ValorTotalAlojamiento,
    IReadOnlyList<ConsumoDto> Consumos,
    decimal TotalServicios,
    IReadOnlyList<ReservaTourDto> Tours,
    decimal TotalTours,
    decimal MontoConsolidado,
    bool CuentaCerrada,
    int? IdPrefactura,
    DateTime? FechaGeneracion,
    string? MetodoPago);

/// <summary>Datos del pago con el que recepción cierra la cuenta en el check-out (HU-06).</summary>
public sealed record SolicitudCierreCuenta(
    int HabitacionId,
    decimal MontoPagado,
    TipoPago? MedioPago,
    string? DetallePago,
    int EmpleadoId);
