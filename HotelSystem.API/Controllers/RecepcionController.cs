using HotelSystem.API.Contratos;
using HotelSystem.API.Seguridad;
using HotelSystem.Application.Reservas;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.API.Controllers;

/// <summary>Check-in del dia (HU-06). El check-out se hace al cerrar la cuenta (CuentasController).</summary>
[Route("api/recepcion")]
[Authorize(Roles = RolesApi.Recepcion)]
public sealed class RecepcionController(IReservaService reservas) : ApiControllerBase
{
    [HttpGet("llegadas-hoy")]
    public IEnumerable<MovimientoRespuesta> LlegadasHoy() =>
        reservas.LlegadasDeHoy().Select(m => MovimientoRespuesta.Desde(m, m.Estado == EstadoReserva.EnCurso));

    [HttpGet("salidas-hoy")]
    public IEnumerable<MovimientoRespuesta> SalidasHoy() =>
        reservas.SalidasDeHoy().Select(m => MovimientoRespuesta.Desde(m, m.Estado == EstadoReserva.Finalizada));

    /// <summary>Registra el check-in: la habitacion debe estar DISPONIBLE y queda OCUPADA (RN-02).</summary>
    [HttpPost("checkin/{idReserva}")]
    public MovimientoRespuesta CheckIn(string idReserva)
    {
        if (!ReservaHabitacion.TryIdDesdeCodigo(idReserva, out var id))
            throw new EntidadNoEncontradaException($"La reserva {idReserva} no existe.");

        var movimiento = reservas.RegistrarCheckIn(id, UsuarioId);
        return MovimientoRespuesta.Desde(movimiento, realizado: true);
    }
}
