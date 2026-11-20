using HotelSystem.API.Contratos;
using HotelSystem.API.Infraestructura;
using HotelSystem.API.Seguridad;
using HotelSystem.Application.ServiciosAdicionales;
using HotelSystem.Domain.Enumeraciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.API.Controllers;

/// <summary>Catalogo de servicios adicionales (HU-07).</summary>
[Route("api/servicios")]
public sealed class ServiciosController(IServicioAdicionalService servicios) : ApiControllerBase
{
    /// <summary>Por defecto solo los activos; ?todos=true incluye los inactivos.</summary>
    [HttpGet]
    public IReadOnlyList<ServicioAdicionalDto> Catalogo([FromQuery] bool todos = false) =>
        servicios.ListarCatalogo(soloActivos: !todos);
}

/// <summary>Solicitudes de servicio del huesped (HU-07) y su atencion por el personal.</summary>
[Route("api/solicitudes-servicio")]
public sealed class SolicitudesServicioController(IServicioAdicionalService servicios) : ApiControllerBase
{
    /// <summary>Requiere check-in activo: la solicitud queda asociada a la habitacion (RF-05).</summary>
    [HttpPost]
    [Authorize(Roles = RolesApi.Huesped)]
    public ActionResult<SolicitudServicioDto> Crear(NuevaSolicitudServicio solicitud) =>
        StatusCode(StatusCodes.Status201Created, servicios.SolicitarServicio(UsuarioId, solicitud.IdServicio));

    [HttpGet("mias")]
    [Authorize(Roles = RolesApi.Huesped)]
    public IReadOnlyList<SolicitudServicioDto> Mias() => servicios.SolicitudesDeHuesped(UsuarioId);

    [HttpGet]
    [Authorize(Roles = $"{RolesApi.Servicios},{RolesApi.Recepcion}")]
    public IReadOnlyList<SolicitudServicioDto> Listar() => servicios.ListarSolicitudes();

    /// <summary>Cambia la solicitud a EN_PROCESO o COMPLETADA.</summary>
    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = $"{RolesApi.Servicios},{RolesApi.Recepcion}")]
    public SolicitudServicioDto CambiarEstado(int id, CambioEstadoSolicitudServicio solicitud) =>
        servicios.ActualizarSolicitud(id, LectorValores.EnumRequerido<EstadoSolicitud>(solicitud.Estado, "estado"), UsuarioId);
}

/// <summary>Consumos cargados a la cuenta de la habitacion (HU-08, RF-11).</summary>
[Route("api/consumos")]
public sealed class ConsumosController(IServicioAdicionalService servicios) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = $"{RolesApi.Servicios},{RolesApi.Recepcion},{RolesApi.Administrador}")]
    public IReadOnlyList<ConsumoDto> Listar() => servicios.ListarConsumos();

    [HttpGet("mios")]
    [Authorize(Roles = RolesApi.Huesped)]
    public IReadOnlyList<ConsumoDto> Mios() => servicios.ConsumosDeHuesped(UsuarioId);

    /// <summary>Solo consumos de hoy (RN-06) y solo con la cuenta de la estancia abierta (RN-07).</summary>
    [HttpPost]
    [Authorize(Roles = RolesApi.Servicios)]
    public ActionResult<ConsumoDto> Registrar(ConsumoSolicitud solicitud)
    {
        var consumo = servicios.RegistrarConsumo(new RegistroConsumo(
            solicitud.NumeroHabitacion ?? string.Empty,
            solicitud.IdServicio,
            solicitud.NombreServicio,
            solicitud.Cantidad ?? 1,
            solicitud.PrecioAplicado,
            solicitud.Fecha,
            UsuarioId));

        return StatusCode(StatusCodes.Status201Created, consumo);
    }
}
