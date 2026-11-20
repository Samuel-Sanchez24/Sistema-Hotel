using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;
using HotelSystem.Domain.Interfaces;

namespace HotelSystem.Domain.Entidades;

/// <summary>
/// Cuenta única de una estancia (RN-07): reúne el alojamiento, los consumos de servicios
/// adicionales y los tours. Se abre al confirmar la reserva y se cierra en el check-out;
/// una cuenta cerrada no admite más consumos.
///
/// Implementa <see cref="ICalculable"/> (tiene un total) e <see cref="IPagable"/> (se paga).
/// </summary>
public class Cuenta : ICalculable, IPagable
{
    private Cuenta()
    {
    }

    public Cuenta(ReservaHabitacion reserva, DateTime ahora)
    {
        Reserva = reserva;
        ReservaHabitacionId = reserva.Id;
        Estado = EstadoCuenta.Abierta;
        FechaApertura = ahora;
        reserva.AsociarCuenta(this);
    }

    public int Id { get; private set; }

    public int ReservaHabitacionId { get; private set; }

    public ReservaHabitacion Reserva { get; private set; } = null!;

    public EstadoCuenta Estado { get; private set; }

    public DateTime FechaApertura { get; private set; }

    public DateTime? FechaCierre { get; private set; }

    public TipoPago? MedioPago { get; private set; }

    public string? DetallePago { get; private set; }

    public decimal MontoPagado { get; private set; }

    public List<ConsumoServicio> Consumos { get; private set; } = new();

    public List<ReservaTour> ReservasTour { get; private set; } = new();

    public bool EstaAbierta => Estado == EstadoCuenta.Abierta;

    public bool EstaPagada => Estado == EstadoCuenta.Cerrada;

    /* ─── Monto consolidado (RN-07) ─── */

    public decimal TotalAlojamiento => Reserva.ValorTotalAlojamiento;

    public decimal TotalServicios => Consumos.Sum(c => c.Subtotal);

    /// <summary>Los tours cancelados no se cobran.</summary>
    public decimal TotalTours => ReservasTour.Where(t => t.EstaVigente).Sum(t => t.CalcularTotal());

    /// <summary>Alojamiento + servicios adicionales + tours.</summary>
    public decimal CalcularTotal() => TotalAlojamiento + TotalServicios + TotalTours;

    /* ─── Operaciones ─── */

    /// <summary>RN-07: solo una cuenta abierta admite consumos.</summary>
    public ConsumoServicio RegistrarConsumo(ServicioAdicional servicio, int cantidad, decimal precioAplicado, DateTime fechaHora, int? registradoPorId)
    {
        ExigirAbierta("registrar consumos");
        var consumo = new ConsumoServicio(this, servicio, cantidad, precioAplicado, fechaHora, registradoPorId);
        Consumos.Add(consumo);
        return consumo;
    }

    /// <summary>Carga un tour a la cuenta. Lo llama <see cref="ReservaTour"/> al crearse.</summary>
    internal void AgregarTour(ReservaTour reservaTour)
    {
        ExigirAbierta("cargar tours");
        if (!ReservasTour.Contains(reservaTour)) ReservasTour.Add(reservaTour);
    }

    /// <summary>
    /// Registra el pago y cierra la cuenta (HU-06). El monto debe coincidir con el total
    /// consolidado: no hay pagos parciales ni saldos a favor.
    /// </summary>
    public void RegistrarPago(decimal monto, TipoPago medio, string? detalle, DateTime fecha)
    {
        ExigirAbierta("registrar el pago");
        if (monto != CalcularTotal())
            throw new ReglaNegocioException("El monto pagado debe coincidir con el total de la cuenta (HU-06 Esc.5).");

        MontoPagado = monto;
        MedioPago = medio;
        DetallePago = string.IsNullOrWhiteSpace(detalle) ? null : detalle.Trim();
        FechaCierre = fecha;
        Estado = EstadoCuenta.Cerrada;
    }

    /// <summary>La reserva se canceló antes del check-in: la cuenta queda sin efecto.</summary>
    public void Anular(DateTime fecha)
    {
        ExigirAbierta("anularla");
        FechaCierre = fecha;
        Estado = EstadoCuenta.Anulada;
    }

    private void ExigirAbierta(string accion)
    {
        if (!EstaAbierta)
            throw new ReglaNegocioException($"La cuenta ya esta cerrada: no se puede {accion} (RN-07).", "RN-07");
    }
}
