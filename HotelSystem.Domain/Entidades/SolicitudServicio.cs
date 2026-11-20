using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Domain.Entidades;

/// <summary>
/// Solicitud de un servicio adicional hecha por el huésped desde la web (HU-07).
/// Queda asociada a la cuenta de su estancia, y por tanto a su habitación (RF-05).
/// </summary>
public class SolicitudServicio
{
    private SolicitudServicio()
    {
    }

    public SolicitudServicio(Cuenta cuenta, ServicioAdicional servicio, DateTime ahora)
    {
        if (!cuenta.EstaAbierta)
            throw new ReglaNegocioException("La cuenta ya esta cerrada: no se pueden solicitar servicios (RN-07).", "RN-07");
        if (!servicio.Activo)
            throw new ReglaNegocioException($"El servicio {servicio.Nombre} no esta disponible.");

        Cuenta = cuenta;
        CuentaId = cuenta.Id;
        Servicio = servicio;
        ServicioAdicionalId = servicio.Id;
        Estado = EstadoSolicitud.Pendiente;
        FechaSolicitud = ahora;
    }

    public int Id { get; private set; }

    public int CuentaId { get; private set; }

    public Cuenta Cuenta { get; private set; } = null!;

    public int ServicioAdicionalId { get; private set; }

    public ServicioAdicional Servicio { get; private set; } = null!;

    public EstadoSolicitud Estado { get; private set; }

    public DateTime FechaSolicitud { get; private set; }

    public DateTime? FechaAtencion { get; private set; }

    public void Atender(DateTime ahora)
    {
        if (Estado != EstadoSolicitud.Pendiente)
            throw new ReglaNegocioException("La solicitud ya fue atendida.");
        Estado = EstadoSolicitud.EnProceso;
        FechaAtencion = ahora;
    }

    public void Completar(DateTime ahora)
    {
        if (Estado == EstadoSolicitud.Completada)
            throw new ReglaNegocioException("La solicitud ya esta completada.");
        Estado = EstadoSolicitud.Completada;
        FechaAtencion = ahora;
    }
}
