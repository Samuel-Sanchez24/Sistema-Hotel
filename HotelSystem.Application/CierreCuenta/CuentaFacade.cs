using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.Comun;
using HotelSystem.Application.Reservas;
using HotelSystem.Application.ServiciosAdicionales;
using HotelSystem.Application.Tours;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Application.CierreCuenta;

/// <summary>
/// Patrón Facade. El check-out toca cuatro partes del sistema: la cuenta, los consumos de
/// Servicios Adicionales, los tours y la reserva con su habitación. Esta fachada las coordina
/// y ofrece a la capa de presentación dos operaciones simples: <see cref="GenerarPrefactura"/>
/// y <see cref="CerrarCuenta"/>. El controlador de recepción solo conoce esta clase (a través
/// de <see cref="ICuentaService"/>); no sabe qué servicios intervienen ni en qué orden.
///
/// La fachada no contiene reglas propias: delega en los servicios de cada módulo y en las
/// entidades. Lo único que hace es ordenar los pasos y garantizar que ocurran todos o ninguno.
/// </summary>
public sealed class CuentaFacade(
    ICuentaRepository cuentas,
    IHabitacionRepository habitaciones,
    IReservaService reservas,
    ITourService tours,
    IServicioAdicionalService servicios,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    Bitacora bitacora) : ICuentaService
{
    /* ─── HU-12: consulta de la cuenta consolidada ─── */

    public PrefacturaDto? ConsultarPorHabitacion(int habitacionId)
    {
        var cuenta = cuentas.ObtenerAbiertaPorHabitacion(habitacionId);
        return cuenta is null ? null : Consolidar(cuenta, null);
    }

    public PrefacturaDto? ConsultarPorNumero(string numeroHabitacion)
    {
        if (string.IsNullOrWhiteSpace(numeroHabitacion)) return null;

        var habitacion = habitaciones.ObtenerPorNumero(numeroHabitacion.Trim());
        return habitacion is null ? null : ConsultarPorHabitacion(habitacion.Id);
    }

    public PrefacturaDto? ConsultarDeHuesped(int huespedId)
    {
        var cuenta = cuentas.ObtenerAbiertaPorHuesped(huespedId);
        return cuenta is null ? null : Consolidar(cuenta, null);
    }

    public PrefacturaDto GenerarPrefactura(int habitacionId, int empleadoId)
    {
        var cuenta = BuscarCuentaAbierta(habitacionId);

        var documento = new Prefactura(cuenta, reloj.Ahora, esFinal: false);
        cuentas.AgregarPrefactura(documento);

        bitacora.Registrar(empleadoId, TipoOperacion.Prefactura,
            $"Prefactura Hab. {cuenta.Reserva.Habitacion.Numero}: {Formato.Dinero(documento.MontoTotal)}");
        unidadDeTrabajo.GuardarCambios();

        return Consolidar(cuenta, documento);
    }

    /* ─── HU-06: check-out ─── */

    public PrefacturaDto CerrarCuenta(SolicitudCierreCuenta solicitud)
    {
        if (solicitud.MedioPago is not { } medioPago)
            throw new ReglaNegocioException("Seleccione un metodo de pago para cerrar la cuenta (RN-07 - HU-06).", "RN-07");

        // Todo el cierre va en una transacción: si un paso falla, no queda nada a medias.
        return unidadDeTrabajo.EnTransaccion(() =>
        {
            // 1. Cuenta única de la estancia en curso (RN-07).
            var cuenta = BuscarCuentaAbierta(solicitud.HabitacionId);

            // 2. Monto consolidado: alojamiento + servicios adicionales + tours.
            var total = cuenta.CalcularTotal();

            // 3. El pago debe cubrir exactamente ese monto.
            if (solicitud.MontoPagado != total)
                throw new ReglaNegocioException($"El monto pagado debe coincidir con la prefactura: {Formato.Dinero(total)} (HU-06 Esc.5).");

            // 4. Se registra el pago y la cuenta queda cerrada: ya no admite consumos.
            var ahora = reloj.Ahora;
            cuenta.RegistrarPago(solicitud.MontoPagado, medioPago, solicitud.DetallePago, ahora);

            // 5. Módulo Reservas: la reserva queda finalizada y la habitación pasa a limpieza (RN-02).
            reservas.RegistrarSalida(cuenta.ReservaHabitacionId, solicitud.EmpleadoId);

            // 6. Prefactura final como constancia del cierre.
            var documento = new Prefactura(cuenta, ahora, esFinal: true);
            cuentas.AgregarPrefactura(documento);
            unidadDeTrabajo.GuardarCambios();

            return Consolidar(cuenta, documento);
        });
    }

    /* ─── Auxiliares ─── */

    private Cuenta BuscarCuentaAbierta(int habitacionId) =>
        cuentas.ObtenerAbiertaPorHabitacion(habitacionId)
            ?? throw new ReglaNegocioException("La habitacion no tiene una estancia en curso con cuenta abierta (RN-07).", "RN-07");

    /// <summary>
    /// Arma la vista consolidada pidiendo a cada módulo su parte: los consumos a Servicios
    /// Adicionales y los tours a Tours. Los totales los calcula la propia <see cref="Cuenta"/>.
    /// </summary>
    private PrefacturaDto Consolidar(Cuenta cuenta, Prefactura? documento)
    {
        var reserva = cuenta.Reserva;
        var consumos = servicios.ConsumosDeCuenta(cuenta.Id);
        var toursCargados = tours.ReservasDeCuenta(cuenta.Id);

        return new PrefacturaDto(
            CuentaId: cuenta.Id,
            ReservaId: reserva.Id,
            CodigoReserva: reserva.Codigo,
            HabitacionId: reserva.HabitacionId,
            NumeroHabitacion: reserva.Habitacion.Numero,
            TipoHabitacion: reserva.Habitacion.Tipo,
            NombreHuesped: reserva.Huesped.Nombre,
            FechaEntrada: reserva.FechaEntrada,
            FechaSalida: reserva.FechaSalida,
            Noches: reserva.Noches,
            PrecioNoche: reserva.PrecioNoche,
            ValorTotalAlojamiento: cuenta.TotalAlojamiento,
            Consumos: consumos,
            TotalServicios: cuenta.TotalServicios,
            Tours: toursCargados,
            TotalTours: cuenta.TotalTours,
            MontoConsolidado: cuenta.CalcularTotal(),
            CuentaCerrada: !cuenta.EstaAbierta,
            IdPrefactura: documento?.Id,
            FechaGeneracion: documento?.FechaGeneracion,
            MetodoPago: cuenta.DetallePago ?? cuenta.MedioPago?.ToString());
    }
}
