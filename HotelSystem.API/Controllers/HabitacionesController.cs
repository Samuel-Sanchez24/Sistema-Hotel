using HotelSystem.API.Contratos;
using HotelSystem.API.Infraestructura;
using HotelSystem.API.Seguridad;
using HotelSystem.Application.Housekeeping;
using HotelSystem.Application.Reservas;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.API.Controllers;

/// <summary>Estado de las habitaciones (HU-05, HU-11) y busqueda de disponibilidad (HU-03).</summary>
[Route("api/habitaciones")]
public sealed class HabitacionesController(IHabitacionService habitaciones, IReservaService reservas) : ApiControllerBase
{
    private const string Personal = $"{RolesApi.Recepcion},{RolesApi.Housekeeping},{RolesApi.Servicios},{RolesApi.Administrador}";

    /// <summary>Panel de habitaciones con su estado actual (HU-05).</summary>
    [HttpGet]
    [Authorize(Roles = Personal)]
    public IReadOnlyList<HabitacionDto> Listar() => habitaciones.Listar();

    [HttpGet("{id:int}")]
    [Authorize(Roles = Personal)]
    public ActionResult<HabitacionDto> Obtener(int id) =>
        habitaciones.Obtener(id) ?? throw new EntidadNoEncontradaException("La habitacion no existe.");

    /// <summary>
    /// Habitaciones libres en todo el rango (RN-01), con capacidad para los huespedes (RN-11).
    /// Ejemplo: /api/habitaciones/disponibles?tipo=DOBLE&amp;fechaIngreso=2026-12-01&amp;fechaSalida=2026-12-04&amp;cantidadHuespedes=2
    /// </summary>
    [HttpGet("disponibles")]
    [Authorize(Roles = $"{RolesApi.Huesped},{RolesApi.Recepcion}")]
    public IReadOnlyList<HabitacionDto> Disponibles(
        [FromQuery] string? tipo,
        [FromQuery] DateOnly? fechaIngreso,
        [FromQuery] DateOnly? fechaSalida,
        [FromQuery] int cantidadHuespedes = 1)
    {
        var tipoHabitacion = LectorValores.EnumRequerido<TipoHabitacion>(tipo, "tipo");
        var entrada = Requerido(fechaIngreso, "Indique la fecha de ingreso (AAAA-MM-DD).");
        var salida = Requerido(fechaSalida, "Indique la fecha de salida (AAAA-MM-DD).");

        return reservas.BuscarDisponibles(tipoHabitacion, entrada, salida, Math.Max(1, cantidadHuespedes));
    }

    /// <summary>
    /// Housekeeping cambia el estado (patron State: las transiciones no permitidas responden 400, RN-02).
    /// Para MANTENIMIENTO la nota de la incidencia es obligatoria (HU-11 Esc.3).
    /// </summary>
    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = RolesApi.Housekeeping)]
    public HabitacionDto CambiarEstado(int id, CambioEstadoHabitacionSolicitud solicitud)
    {
        var estado = LectorValores.EnumRequerido<EstadoHabitacion>(solicitud.Estado, "estado");
        return habitaciones.CambiarEstado(id, estado, solicitud.Nota, UsuarioId);
    }
}
