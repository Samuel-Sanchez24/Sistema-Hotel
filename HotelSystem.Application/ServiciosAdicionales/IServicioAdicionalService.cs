using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Application.ServiciosAdicionales;

/// <summary>Servicios adicionales: solicitudes del huésped (HU-07) y registro de consumos (HU-08).</summary>
public interface IServicioAdicionalService
{
    IReadOnlyList<ServicioAdicionalDto> ListarCatalogo(bool soloActivos = false);

    /// <summary>El huésped con check-in activo solicita un servicio; queda asociado a su habitación (RF-05).</summary>
    SolicitudServicioDto SolicitarServicio(int huespedId, int servicioId);

    /// <summary>Solicitudes de la estancia en curso del huésped.</summary>
    IReadOnlyList<SolicitudServicioDto> SolicitudesDeHuesped(int huespedId);

    IReadOnlyList<SolicitudServicioDto> ListarSolicitudes();

    SolicitudServicioDto ActualizarSolicitud(int solicitudId, EstadoSolicitud estado, int empleadoId);

    /// <summary>
    /// Carga un consumo a la cuenta de la habitación (RF-11). Solo acepta consumos del día
    /// (RN-06) y solo si la cuenta de la estancia sigue abierta (RN-07).
    /// </summary>
    ConsumoDto RegistrarConsumo(RegistroConsumo registro);

    /// <summary>Historial de todos los consumos, del más reciente al más antiguo.</summary>
    IReadOnlyList<ConsumoDto> ListarConsumos();

    /// <summary>Consumos de la estancia en curso del huésped.</summary>
    IReadOnlyList<ConsumoDto> ConsumosDeHuesped(int huespedId);

    /// <summary>Consumos de una cuenta. Lo usa la fachada de cierre de cuenta.</summary>
    IReadOnlyList<ConsumoDto> ConsumosDeCuenta(int cuentaId);
}
