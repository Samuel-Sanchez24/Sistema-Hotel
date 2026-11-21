using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Application.Tours;

/// <summary>Tour con sus cupos y los recursos asignados (HU-09, HU-10).</summary>
public sealed record TourResumenDto(
    int Id,
    string Nombre,
    string Destino,
    DateOnly Fecha,
    TimeOnly HorarioInicio,
    TimeOnly HorarioFin,
    int CupoMaximo,
    int CuposReservados,
    int CuposDisponibles,
    decimal PrecioPorPersona,
    int? GuiaId,
    string? NombreGuia,
    int? VehiculoId,
    string? NombreVehiculo)
{
    public bool EstaLleno => CuposDisponibles <= 0;

    public static TourResumenDto Desde(Tour tour) => new(
        tour.Id,
        tour.Nombre,
        tour.Destino,
        tour.Fecha,
        tour.HorarioInicio,
        tour.HorarioFin,
        tour.CupoMaximo,
        tour.CuposReservados,
        tour.CuposDisponibles,
        tour.PrecioPorPersona,
        tour.GuiaId,
        tour.Guia?.Nombre,
        tour.VehiculoId,
        tour.Vehiculo?.Nombre);
}

/// <param name="Ocupado">Ya está asignado a otro tour que se cruza de horario con el consultado (RN-03).</param>
public sealed record GuiaDto(int Id, string Nombre, bool Ocupado);

/// <param name="Ocupado">Ya está asignado a otro tour que se cruza de horario con el consultado (RN-03).</param>
public sealed record VehiculoDto(int Id, string Nombre, string Placa, string Modelo, int CapacidadPasajeros, bool Ocupado);

public sealed record RecursosDto(IReadOnlyList<GuiaDto> Guias, IReadOnlyList<VehiculoDto> Vehiculos);

public sealed record RecursosLiberadosDto(string NombreTour, string? NombreGuia, string? NombreVehiculo);

/// <summary>Datos que envía el huésped para reservar un tour (HU-09).</summary>
public sealed record SolicitudReservaTour(
    int HuespedId,
    int TourId,
    int NumeroPersonas,
    bool MayorDeEdadConfirmado,
    TipoPago? MedioPago,
    string? DetallePago);

public sealed record ReservaTourDto(
    int Id,
    string Codigo,
    int TourId,
    string NombreTour,
    string Destino,
    DateOnly Fecha,
    TimeOnly HorarioInicio,
    int NumeroPersonas,
    decimal PrecioPorPersona,
    decimal ValorTotalTour,
    EstadoReserva Estado)
{
    public bool EstaVigente => Estado != EstadoReserva.Cancelada;

    public static ReservaTourDto Desde(ReservaTour reserva) => new(
        reserva.Id,
        reserva.Codigo,
        reserva.TourId,
        reserva.Tour.Nombre,
        reserva.Tour.Destino,
        reserva.Tour.Fecha,
        reserva.Tour.HorarioInicio,
        reserva.NumeroPersonas,
        reserva.PrecioPorPersona,
        reserva.CalcularTotal(),
        reserva.Estado);
}
