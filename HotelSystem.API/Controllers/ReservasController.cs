using HotelSystem.API.Contratos;
using HotelSystem.API.Infraestructura;
using HotelSystem.API.Seguridad;
using HotelSystem.Application.Housekeeping;
using HotelSystem.Application.Reservas;
using HotelSystem.Application.Tours;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.API.Controllers;

/// <summary>Reservas del huesped (HU-03) y correo de confirmacion (HU-04).</summary>
[Route("api/reservas")]
[Authorize(Roles = RolesApi.Huesped)]
public sealed class ReservasController(
    IReservaService reservas,
    ITourService tours,
    IHabitacionService habitaciones) : ApiControllerBase
{
    /// <summary>Reservas de habitacion y de tour del huesped, con el cargo que tendria cancelarlas ahora (RN-09).</summary>
    [HttpGet("mias")]
    public IReadOnlyList<ReservaHuespedDto> Mias() => reservas.ReservasDeHuesped(UsuarioId);

    /// <summary>
    /// Confirma la reserva: valida RN-10, RN-04, RN-11 y RN-01 dentro de una transaccion,
    /// abre la cuenta de la estancia (RN-07) y envia el correo de confirmacion (HU-04).
    /// </summary>
    [HttpPost("habitacion")]
    public ActionResult<ReservaConfirmadaDto> ReservarHabitacion(ReservaHabitacionSolicitud solicitud)
    {
        var habitacionId = IdHabitacion(habitaciones, solicitud.NumeroHabitacion);
        var (medio, detalle) = LectorValores.MedioPago(solicitud.MetodoPago, solicitud.DetallePago);

        var confirmada = reservas.ReservarHabitacion(new SolicitudReservaHabitacion(
            UsuarioId,
            habitacionId,
            Requerido(solicitud.FechaIngreso, "Indique la fecha de ingreso (AAAA-MM-DD)."),
            Requerido(solicitud.FechaSalida, "Indique la fecha de salida (AAAA-MM-DD)."),
            Math.Max(1, solicitud.CantidadHuespedes),
            solicitud.MayorDeEdad,
            medio,
            detalle));

        return StatusCode(StatusCodes.Status201Created, confirmada);
    }

    /// <summary>
    /// Cancela una reserva. Acepta el codigo ("RES-004", "TUR-002") o el numero de la reserva de habitacion.
    /// RN-09: a menos de 48 horas del check-in se cobra una noche.
    /// </summary>
    [HttpPost("{id}/cancelar")]
    public CancelacionRespuesta Cancelar(string id)
    {
        if (ReservaTour.TryIdDesdeCodigo(id, out var reservaTourId))
        {
            var tour = tours.CancelarReserva(reservaTourId, UsuarioId);
            return new CancelacionRespuesta(tour.Codigo, Tardia: false, Cargo: 0m);
        }

        if (!ReservaHabitacion.TryIdDesdeCodigo(id, out var reservaId))
            throw new EntidadNoEncontradaException($"La reserva {id} no existe.");

        var resultado = reservas.CancelarReserva(reservaId, UsuarioId);
        return new CancelacionRespuesta(resultado.Codigo, resultado.Tardia, resultado.Cargo);
    }
}
