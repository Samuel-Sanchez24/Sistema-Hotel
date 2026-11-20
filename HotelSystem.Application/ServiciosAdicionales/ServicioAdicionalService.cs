using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.Comun;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Application.ServiciosAdicionales;

public sealed class ServicioAdicionalService(
    IServicioAdicionalRepository servicios,
    IHabitacionRepository habitaciones,
    ICuentaRepository cuentas,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    Bitacora bitacora) : IServicioAdicionalService
{
    /* ─── Catálogo ─── */

    public IReadOnlyList<ServicioAdicionalDto> ListarCatalogo(bool soloActivos = false) =>
        servicios.Listar()
            .Where(s => !soloActivos || s.Activo)
            .Select(ServicioAdicionalDto.Desde)
            .ToList();

    /* ─── HU-07: solicitudes del huésped ─── */

    public SolicitudServicioDto SolicitarServicio(int huespedId, int servicioId)
    {
        var cuenta = cuentas.ObtenerAbiertaPorHuesped(huespedId)
            ?? throw new ReglaNegocioException("Necesita un check-in activo para solicitar servicios (HU-07).");
        var servicio = servicios.ObtenerPorId(servicioId)
            ?? throw new EntidadNoEncontradaException("El servicio no existe.");

        var solicitud = new SolicitudServicio(cuenta, servicio, reloj.Ahora);
        servicios.AgregarSolicitud(solicitud);
        unidadDeTrabajo.GuardarCambios();

        return SolicitudServicioDto.Desde(solicitud);
    }

    public IReadOnlyList<SolicitudServicioDto> SolicitudesDeHuesped(int huespedId)
    {
        var cuenta = cuentas.ObtenerAbiertaPorHuesped(huespedId);
        if (cuenta is null) return [];

        return servicios.ListarSolicitudesPorCuenta(cuenta.Id).Select(SolicitudServicioDto.Desde).ToList();
    }

    public IReadOnlyList<SolicitudServicioDto> ListarSolicitudes() =>
        servicios.ListarSolicitudes().Select(SolicitudServicioDto.Desde).ToList();

    public SolicitudServicioDto ActualizarSolicitud(int solicitudId, EstadoSolicitud estado, int empleadoId)
    {
        var solicitud = servicios.ObtenerSolicitud(solicitudId)
            ?? throw new EntidadNoEncontradaException("La solicitud no existe.");

        switch (estado)
        {
            case EstadoSolicitud.EnProceso:
                solicitud.Atender(reloj.Ahora);
                break;
            case EstadoSolicitud.Completada:
                solicitud.Completar(reloj.Ahora);
                break;
            default:
                throw new ReglaNegocioException("Una solicitud no puede volver a quedar pendiente.");
        }

        bitacora.Registrar(empleadoId, TipoOperacion.AtencionSolicitud,
            $"Solicitud de {solicitud.Servicio.Nombre} - Hab. {solicitud.Cuenta.Reserva.Habitacion.Numero}: {estado}");
        unidadDeTrabajo.GuardarCambios();

        return SolicitudServicioDto.Desde(solicitud);
    }

    /* ─── HU-08: consumos ─── */

    public ConsumoDto RegistrarConsumo(RegistroConsumo registro)
    {
        if (string.IsNullOrWhiteSpace(registro.NumeroHabitacion))
            throw new ReglaNegocioException("Complete todos los campos.");

        var numero = registro.NumeroHabitacion.Trim();
        var habitacion = habitaciones.ObtenerPorNumero(numero)
            ?? throw new EntidadNoEncontradaException($"La habitacion {numero} no existe.");

        // RN-06: un consumo se registra el mismo día en que se presta.
        var hoy = reloj.Hoy;
        if ((registro.Fecha ?? hoy) != hoy)
            throw new ReglaNegocioException($"Solo se registran consumos del dia de hoy ({Formato.FechaConAnio(hoy)}). Un consumo de otra fecha no se acepta (RN-06).", "RN-06");

        // RN-07: tiene que haber una estancia en curso con la cuenta abierta.
        var cuenta = cuentas.ObtenerAbiertaPorHabitacion(habitacion.Id)
            ?? throw new ReglaNegocioException($"La habitacion {numero} no tiene una estancia en curso con cuenta abierta: no se pueden registrar consumos (RN-07).", "RN-07");

        var servicio = BuscarServicio(registro);
        if (!servicio.Activo)
            throw new ReglaNegocioException($"El servicio {servicio.Nombre} no esta disponible.");

        var ahora = reloj.Ahora;
        var consumo = cuenta.RegistrarConsumo(servicio, registro.Cantidad, registro.PrecioAplicado, ahora, registro.EmpleadoId > 0 ? registro.EmpleadoId : null);

        // Si el huésped había solicitado este servicio, la solicitud más antigua queda atendida.
        var solicitudPendiente = servicios.ListarSolicitudesPorCuenta(cuenta.Id)
            .Where(s => s.ServicioAdicionalId == servicio.Id && s.Estado != EstadoSolicitud.Completada)
            .OrderBy(s => s.FechaSolicitud)
            .FirstOrDefault();
        solicitudPendiente?.Completar(ahora);

        bitacora.Registrar(registro.EmpleadoId, TipoOperacion.RegistroConsumo,
            $"{servicio.Nombre} {Formato.Dinero(consumo.Subtotal)} - Hab. {numero}");
        unidadDeTrabajo.GuardarCambios();

        return ConsumoDto.Desde(consumo);
    }

    public IReadOnlyList<ConsumoDto> ListarConsumos() =>
        cuentas.ListarConsumos().Select(ConsumoDto.Desde).ToList();

    public IReadOnlyList<ConsumoDto> ConsumosDeHuesped(int huespedId)
    {
        var cuenta = cuentas.ObtenerAbiertaPorHuesped(huespedId);
        if (cuenta is null) return [];

        return EnOrden(cuenta.Consumos);
    }

    public IReadOnlyList<ConsumoDto> ConsumosDeCuenta(int cuentaId)
    {
        var cuenta = cuentas.ObtenerPorId(cuentaId);
        if (cuenta is null) return [];

        return EnOrden(cuenta.Consumos);
    }

    /* ─── Auxiliares ─── */

    private ServicioAdicional BuscarServicio(RegistroConsumo registro)
    {
        ServicioAdicional? servicio = null;
        if (registro.ServicioId is { } id)
        {
            servicio = servicios.ObtenerPorId(id);
        }
        else if (!string.IsNullOrWhiteSpace(registro.NombreServicio))
        {
            servicio = servicios.ObtenerPorNombre(registro.NombreServicio.Trim());
        }

        return servicio ?? throw new EntidadNoEncontradaException("El servicio no existe en el catalogo.");
    }

    private static List<ConsumoDto> EnOrden(IEnumerable<ConsumoServicio> consumos) =>
        consumos.OrderBy(c => c.FechaHora).Select(ConsumoDto.Desde).ToList();
}
