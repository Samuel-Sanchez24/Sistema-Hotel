using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.Comun;
using HotelSystem.Application.Reservas;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Application.Housekeeping;

public sealed class HabitacionService(
    IHabitacionRepository habitaciones,
    IReservaHabitacionRepository reservas,
    SuscriptorHabitaciones suscriptor,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    Bitacora bitacora) : IHabitacionService
{
    public IReadOnlyList<HabitacionDto> Listar()
    {
        var estancias = EstanciasPorHabitacion();
        return habitaciones.Listar()
            .Select(h => HabitacionDto.Desde(h, estancias.GetValueOrDefault(h.Id)))
            .ToList();
    }

    public HabitacionDto? Obtener(int id)
    {
        var habitacion = habitaciones.ObtenerPorId(id);
        if (habitacion is null) return null;
        return HabitacionDto.Desde(habitacion, EstanciasPorHabitacion().GetValueOrDefault(id));
    }

    public HabitacionDto CambiarEstado(int habitacionId, EstadoHabitacion nuevoEstado, string? nota, int empleadoId)
    {
        var habitacion = habitaciones.ObtenerPorId(habitacionId)
            ?? throw new EntidadNoEncontradaException("La habitacion no existe.");
        var ahora = reloj.Ahora;

        // Observer: los paneles quedan suscritos antes de que la habitación cambie.
        suscriptor.Suscribir(habitacion);

        // State: cada llamada la resuelve el estado actual de la habitación, que lanza
        // una ReglaNegocioException si la transición no está permitida (RN-02).
        switch (nuevoEstado)
        {
            case EstadoHabitacion.Disponible:
                habitacion.MarcarDisponible(ahora);
                break;
            case EstadoHabitacion.EnLimpieza:
                habitacion.EnviarALimpieza(ahora);
                break;
            case EstadoHabitacion.Mantenimiento:
                habitacion.EnviarAMantenimiento(nota, ahora);
                break;
            default:
                throw new ReglaNegocioException("El estado Ocupada solo se asigna al registrar el check-in (RN-02).", "RN-02");
        }

        var tipo = nuevoEstado == EstadoHabitacion.Mantenimiento ? TipoOperacion.Incidencia : TipoOperacion.CambioEstadoHabitacion;
        bitacora.Registrar(empleadoId, tipo, $"Hab. {habitacion.Numero}: {habitacion.EstadoActual.Nombre}");
        unidadDeTrabajo.GuardarCambios();

        return HabitacionDto.Desde(habitacion);
    }

    public IReadOnlyList<TareaLimpiezaDto> PlanLimpieza()
    {
        var conLlegadaHoy = reservas.ListarLlegadas(reloj.Hoy).Select(r => r.HabitacionId).ToHashSet();

        return habitaciones.Listar()
            .Where(h => h.Estado == EstadoHabitacion.EnLimpieza)
            .OrderBy(h => conLlegadaHoy.Contains(h.Id) ? 0 : 1)
            .ThenBy(h => h.UltimaActualizacion)
            .Select(h => new TareaLimpiezaDto(HabitacionDto.Desde(h), conLlegadaHoy.Contains(h.Id)))
            .ToList();
    }

    /// <summary>Estancia en curso de cada habitación ocupada, para mostrar huésped y fechas.</summary>
    private Dictionary<int, ReservaHabitacion> EstanciasPorHabitacion() =>
        reservas.ListarEnCurso()
            .GroupBy(r => r.HabitacionId)
            .ToDictionary(g => g.Key, g => g.First());
}
