using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Application.Housekeeping;

/// <summary>Habitación con su estado y, si está ocupada, los datos de la estancia en curso.</summary>
public sealed record HabitacionDto(
    int Id,
    string Numero,
    TipoHabitacion Tipo,
    EstadoHabitacion Estado,
    int Piso,
    int Capacidad,
    decimal PrecioNoche,
    DateTime UltimaActualizacion,
    string? NotaMantenimiento,
    string? HuespedActual,
    DateOnly? FechaEntrada,
    DateOnly? FechaSalida)
{
    public static HabitacionDto Desde(Habitacion habitacion, ReservaHabitacion? estancia = null) => new(
        habitacion.Id,
        habitacion.Numero,
        habitacion.Tipo,
        habitacion.Estado,
        habitacion.Piso,
        habitacion.Capacidad,
        habitacion.PrecioNoche,
        habitacion.UltimaActualizacion,
        habitacion.NotaMantenimiento,
        estancia?.Huesped.Nombre,
        estancia?.FechaEntrada,
        estancia?.FechaSalida);
}

/// <summary>Tarea del plan de limpieza (HU-11, RF-10). Las que tienen llegada hoy van primero.</summary>
public sealed record TareaLimpiezaDto(HabitacionDto Habitacion, bool LlegadaHoy);
