using HotelSystem.API.Contratos;
using HotelSystem.API.Infraestructura;
using HotelSystem.API.Seguridad;
using HotelSystem.Application.CierreCuenta;
using HotelSystem.Application.Housekeeping;
using HotelSystem.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.API.Controllers;

/// <summary>
/// Cuenta consolidada (HU-12, RN-07) y cierre en el check-out (HU-06). Todas las operaciones
/// pasan por la fachada <c>CuentaFacade</c> (patron Facade).
/// </summary>
[Route("api/cuentas")]
public sealed class CuentasController(ICuentaService cuentas, IHabitacionService habitaciones) : ApiControllerBase
{
    /// <summary>Vista previa de la cuenta: alojamiento (noches reales) + servicios + tours.</summary>
    [HttpGet("{numeroHabitacion}/prefactura")]
    [Authorize(Roles = RolesApi.Recepcion)]
    public PrefacturaDto Prefactura(string numeroHabitacion) =>
        cuentas.ConsultarPorNumero(numeroHabitacion)
        ?? throw new EntidadNoEncontradaException($"La habitacion {numeroHabitacion} no tiene una estancia en curso con cuenta abierta.");

    /// <summary>Genera y guarda la prefactura (queda numerada en la base de datos).</summary>
    [HttpPost("{numeroHabitacion}/prefactura")]
    [Authorize(Roles = RolesApi.Recepcion)]
    public PrefacturaDto GenerarPrefactura(string numeroHabitacion) =>
        cuentas.GenerarPrefactura(IdHabitacion(habitaciones, numeroHabitacion), UsuarioId);

    /// <summary>Cuenta de la estancia en curso del huesped que inicio sesion ("Mi cuenta").</summary>
    [HttpGet("mia/prefactura")]
    [Authorize(Roles = RolesApi.Huesped)]
    public PrefacturaDto MiPrefactura() =>
        cuentas.ConsultarDeHuesped(UsuarioId)
        ?? throw new EntidadNoEncontradaException("No tiene una estancia en curso. La cuenta se abre con el check-in.");

    /// <summary>
    /// Check-out completo en una operacion: valida el pago contra el total, cierra la cuenta,
    /// finaliza la reserva, deja la habitacion EN_LIMPIEZA y genera la prefactura final.
    /// </summary>
    [HttpPost("{numeroHabitacion}/cerrar")]
    [Authorize(Roles = RolesApi.Recepcion)]
    public CierreCuentaRespuesta Cerrar(string numeroHabitacion, CierreCuentaSolicitud solicitud)
    {
        var (medio, detalle) = LectorValores.MedioPago(solicitud.MetodoPago, solicitud.DetallePago);
        var cierre = cuentas.CerrarCuenta(new SolicitudCierreCuenta(
            IdHabitacion(habitaciones, numeroHabitacion), solicitud.MontoPagado, medio, detalle, UsuarioId));

        return CierreCuentaRespuesta.Desde(cierre);
    }
}
