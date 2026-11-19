using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Application.Housekeeping;

/// <summary>
/// Estado de las habitaciones: lo consulta recepción (HU-05) y lo actualiza
/// housekeeping (HU-11, RF-10).
/// </summary>
public interface IHabitacionService
{
    /// <summary>Todas las habitaciones con su estado actual.</summary>
    IReadOnlyList<HabitacionDto> Listar();

    HabitacionDto? Obtener(int id);

    /// <summary>
    /// Cambia el estado de una habitación desde housekeeping. Las transiciones no permitidas
    /// (RN-02) se rechazan con una <c>ReglaNegocioException</c>. Para pasar a mantenimiento
    /// la nota de la incidencia es obligatoria (HU-11 Esc.3).
    /// </summary>
    HabitacionDto CambiarEstado(int habitacionId, EstadoHabitacion nuevoEstado, string? nota, int empleadoId);

    /// <summary>Habitaciones en limpieza: primero las que reciben huésped hoy, luego las que llevan más tiempo esperando.</summary>
    IReadOnlyList<TareaLimpiezaDto> PlanLimpieza();
}
