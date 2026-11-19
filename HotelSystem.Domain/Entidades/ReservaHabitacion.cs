using System.Globalization;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Domain.Entidades;

/// <summary>
/// Reserva de una habitación para un rango de fechas (HU-03).
/// Ciclo de vida: Solicitada -> Confirmada -> EnCurso (check-in) -> Finalizada (check-out),
/// o Cancelada antes del check-in.
/// </summary>
public class ReservaHabitacion
{
    public const string PrefijoCodigo = "RES-";

    private ReservaHabitacion()
    {
    }

    public ReservaHabitacion(Huesped huesped, Habitacion habitacion, DateOnly fechaEntrada, DateOnly fechaSalida, int cantidadHuespedes, DateTime ahora)
    {
        if (fechaSalida <= fechaEntrada)
            throw new ReglaNegocioException("La fecha de salida debe ser posterior a la fecha de entrada.");
        if (!habitacion.AdmiteHuespedes(cantidadHuespedes))
            throw new ReglaNegocioException($"La habitacion {habitacion.Numero} admite maximo {habitacion.Capacidad} personas (RN-11).", "RN-11");

        Huesped = huesped;
        HuespedId = huesped.Id;
        Habitacion = habitacion;
        HabitacionId = habitacion.Id;
        FechaEntrada = fechaEntrada;
        FechaSalida = fechaSalida;
        CantidadHuespedes = cantidadHuespedes;
        PrecioNoche = habitacion.PrecioNoche;
        Estado = EstadoReserva.Solicitada;
        FechaReserva = ahora;
    }

    public int Id { get; private set; }

    public int HuespedId { get; private set; }

    public Huesped Huesped { get; private set; } = null!;

    public int HabitacionId { get; private set; }

    public Habitacion Habitacion { get; private set; } = null!;

    public DateOnly FechaEntrada { get; private set; }

    /// <summary>Día del check-out. La noche de este día ya no se cobra.</summary>
    public DateOnly FechaSalida { get; private set; }

    public int CantidadHuespedes { get; private set; }

    /// <summary>Tarifa vigente al reservar. Se copia para que un cambio de tarifa no altere la reserva.</summary>
    public decimal PrecioNoche { get; private set; }

    public EstadoReserva Estado { get; private set; }

    /// <summary>Respaldo de pago registrado al confirmar (RN-04).</summary>
    public TipoPago? MedioPago { get; private set; }

    public string? DetallePago { get; private set; }

    public DateTime FechaReserva { get; private set; }

    public DateTime? FechaCheckIn { get; private set; }

    public DateTime? FechaCheckOut { get; private set; }

    public DateTime? FechaCancelacion { get; private set; }

    /// <summary>Cargo aplicado al cancelar tarde (RN-09). Cero si no hubo cargo.</summary>
    public decimal CargoCancelacion { get; private set; }

    /// <summary>Cuenta única de la estancia (RN-07). Se abre al confirmar la reserva.</summary>
    public Cuenta? Cuenta { get; private set; }

    /* ─── Valores derivados ─── */

    public string Codigo => PrefijoCodigo + Id.ToString("000", CultureInfo.InvariantCulture);

    /// <summary>Duración de la estancia en noches.</summary>
    public int Noches => FechaSalida.DayNumber - FechaEntrada.DayNumber;

    public decimal ValorTotalAlojamiento => Noches * PrecioNoche;

    /// <summary>Una reserva activa bloquea la habitación en sus fechas (RN-01).</summary>
    public bool EstaActiva => Estado is EstadoReserva.Solicitada or EstadoReserva.Confirmada or EstadoReserva.EnCurso;

    /// <summary>
    /// RN-01: dos estancias se cruzan si comparten al menos una noche. El día de salida de una
    /// puede ser el día de entrada de la siguiente.
    /// </summary>
    public bool SeCruzaCon(DateOnly entrada, DateOnly salida) => FechaEntrada < salida && entrada < FechaSalida;

    /// <summary>La fecha queda dentro de la estancia, contando el día de salida.</summary>
    public bool Cubre(DateOnly fecha) => FechaEntrada <= fecha && fecha <= FechaSalida;

    /// <summary>Enlaza la cuenta de la estancia. Lo llama <see cref="Entidades.Cuenta"/> al crearse.</summary>
    internal void AsociarCuenta(Cuenta cuenta) => Cuenta = cuenta;

    /* ─── Transiciones ─── */

    /// <summary>RN-04: la reserva solo se confirma con un respaldo de pago.</summary>
    public void Confirmar(TipoPago medioPago, string? detallePago)
    {
        if (Estado != EstadoReserva.Solicitada)
            throw new ReglaNegocioException($"La reserva {Codigo} ya fue procesada.");
        MedioPago = medioPago;
        DetallePago = string.IsNullOrWhiteSpace(detallePago) ? null : detallePago.Trim();
        Estado = EstadoReserva.Confirmada;
    }

    public void RegistrarCheckIn(DateTime ahora)
    {
        if (Estado != EstadoReserva.Confirmada)
            throw new ReglaNegocioException($"La reserva {Codigo} no esta pendiente de check-in.");
        Estado = EstadoReserva.EnCurso;
        FechaCheckIn = ahora;
    }

    public void RegistrarSalida(DateTime ahora)
    {
        if (Estado != EstadoReserva.EnCurso)
            throw new ReglaNegocioException($"La reserva {Codigo} no tiene una estancia en curso.");
        Estado = EstadoReserva.Finalizada;
        FechaCheckOut = ahora;
    }

    /// <summary>Solo se cancela antes del check-in. El cargo lo decide la política de cancelación (RN-09).</summary>
    public void Cancelar(decimal cargo, DateTime ahora)
    {
        if (Estado is not (EstadoReserva.Solicitada or EstadoReserva.Confirmada))
            throw new ReglaNegocioException($"La reserva {Codigo} ya no se puede cancelar.");
        if (cargo < 0) throw new ReglaNegocioException("El cargo de cancelacion no puede ser negativo.");
        Estado = EstadoReserva.Cancelada;
        CargoCancelacion = cargo;
        FechaCancelacion = ahora;
    }

    /// <summary>Convierte "RES-048" (o "48") en el identificador numérico.</summary>
    public static bool TryIdDesdeCodigo(string? codigo, out int id)
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(codigo)) return false;
        var texto = codigo.Trim();
        if (texto.StartsWith(PrefijoCodigo, StringComparison.OrdinalIgnoreCase)) texto = texto[PrefijoCodigo.Length..];
        return int.TryParse(texto, NumberStyles.None, CultureInfo.InvariantCulture, out id) && id > 0;
    }
}
