using HotelSystem.API.Contratos;
using HotelSystem.API.Infraestructura;
using HotelSystem.API.Seguridad;
using HotelSystem.Application.Tours;
using HotelSystem.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.API.Controllers;

/// <summary>Tours: reserva del huesped (HU-09) y cupos y recursos del coordinador (HU-10).</summary>
[Route("api/tours")]
public sealed class ToursController(ITourService tours) : ApiControllerBase
{
    [HttpGet]
    public IReadOnlyList<TourResumenDto> Listar() => tours.Listar();

    [HttpGet("{id:int}")]
    public TourResumenDto Obtener(int id) =>
        tours.Obtener(id) ?? throw new EntidadNoEncontradaException("El tour no existe.");

    /// <summary>
    /// Guias y vehiculos. Con ?tourId=N cada recurso indica si esta ocupado en otro tour
    /// que se cruza de horario con ese (RN-03).
    /// </summary>
    [HttpGet("recursos")]
    [Authorize(Roles = RolesApi.Coordinador)]
    public RecursosDto Recursos([FromQuery] int? tourId) => tours.ListarRecursos(tourId);

    /// <summary>Reserva de cupos: se bloquean en el acto (RN-05) y se cargan a la cuenta de la estancia (RN-07).</summary>
    [HttpPost("{id:int}/reservas")]
    [Authorize(Roles = RolesApi.Huesped)]
    public ActionResult<ReservaTourDto> Reservar(int id, ReservaTourSolicitud solicitud)
    {
        var (medio, detalle) = LectorValores.MedioPago(solicitud.MetodoPago, solicitud.DetallePago);
        var reserva = tours.ReservarTour(new SolicitudReservaTour(
            UsuarioId, id, solicitud.NumeroPersonas, solicitud.MayorDeEdad, medio, detalle));

        return StatusCode(StatusCodes.Status201Created, reserva);
    }

    [HttpGet("reservas/mias")]
    [Authorize(Roles = RolesApi.Huesped)]
    public IReadOnlyList<ReservaTourDto> MisReservas() => tours.ReservasDeHuesped(UsuarioId);

    [HttpPost("{id:int}/cupos/bloquear")]
    [Authorize(Roles = RolesApi.Coordinador)]
    public TourResumenDto BloquearCupos(int id, CuposSolicitud solicitud) =>
        tours.BloquearCupos(id, solicitud.Cantidad, UsuarioId);

    [HttpPost("{id:int}/cupos/liberar")]
    [Authorize(Roles = RolesApi.Coordinador)]
    public TourResumenDto LiberarCupos(int id, CuposSolicitud solicitud) =>
        tours.LiberarCupos(id, solicitud.Cantidad, UsuarioId);

    /// <summary>Asigna guia y vehiculo. 400 si alguno ya esta en otro tour con horario cruzado (RN-03).</summary>
    [HttpPut("{id:int}/asignacion")]
    [Authorize(Roles = RolesApi.Coordinador)]
    public TourResumenDto Asignar(int id, AsignacionSolicitud solicitud) =>
        tours.AsignarRecursos(id, solicitud.IdGuia, solicitud.IdVehiculo, UsuarioId);

    [HttpDelete("{id:int}/asignacion")]
    [Authorize(Roles = RolesApi.Coordinador)]
    public RecursosLiberadosDto LiberarRecursos(int id) => tours.LiberarRecursos(id, UsuarioId);
}
