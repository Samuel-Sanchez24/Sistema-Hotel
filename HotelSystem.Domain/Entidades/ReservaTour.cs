using System.Globalization;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;
using HotelSystem.Domain.Interfaces;

namespace HotelSystem.Domain.Entidades;

/// <summary>
/// Reserva de cupos en un tour (HU-09). Se carga a la cuenta única de la estancia del
/// huésped (RN-07), por eso implementa <see cref="ICalculable"/> pero no <c>IPagable</c>.
/// </summary>
public class ReservaTour : ICalculable
{
    public const string PrefijoCodigo = "TUR-";

    private ReservaTour()
    {
    }

    /// <summary>
    /// Crea la reserva ya confirmada y la carga a la cuenta. El bloqueo de cupos (RN-05)
    /// lo hace quien la crea llamando a <see cref="Entidades.Tour.BloquearCupos"/>.
    /// </summary>
    public ReservaTour(Tour tour, Huesped huesped, Cuenta cuenta, int numeroPersonas, TipoPago medioPago, string? detallePago, DateTime ahora)
    {
        if (numeroPersonas < 1) throw new ReglaNegocioException("El numero de personas debe ser de al menos una.");

        Tour = tour;
        TourId = tour.Id;
        Huesped = huesped;
        HuespedId = huesped.Id;
        Cuenta = cuenta;
        CuentaId = cuenta.Id;
        NumeroPersonas = numeroPersonas;
        PrecioPorPersona = tour.PrecioPorPersona;
        MedioPago = medioPago;
        DetallePago = string.IsNullOrWhiteSpace(detallePago) ? null : detallePago.Trim();
        Estado = EstadoReserva.Confirmada;
        FechaReserva = ahora;
        cuenta.AgregarTour(this);
    }

    public int Id { get; private set; }

    public int TourId { get; private set; }

    public Tour Tour { get; private set; } = null!;

    public int HuespedId { get; private set; }

    public Huesped Huesped { get; private set; } = null!;

    public int CuentaId { get; private set; }

    public Cuenta Cuenta { get; private set; } = null!;

    public int NumeroPersonas { get; private set; }

    /// <summary>Precio vigente al reservar.</summary>
    public decimal PrecioPorPersona { get; private set; }

    public EstadoReserva Estado { get; private set; }

    /// <summary>Respaldo de pago registrado al reservar (RN-04).</summary>
    public TipoPago MedioPago { get; private set; }

    public string? DetallePago { get; private set; }

    public DateTime FechaReserva { get; private set; }

    public DateTime? FechaCancelacion { get; private set; }

    public string Codigo => PrefijoCodigo + Id.ToString("000", CultureInfo.InvariantCulture);

    public bool EstaVigente => Estado != EstadoReserva.Cancelada;

    /// <summary>Valor total del tour: personas por precio.</summary>
    public decimal CalcularTotal() => NumeroPersonas * PrecioPorPersona;

    /// <summary>Marca la reserva como cancelada. Liberar los cupos es responsabilidad de quien cancela.</summary>
    public void Cancelar(DateTime ahora)
    {
        if (Estado == EstadoReserva.Cancelada)
            throw new ReglaNegocioException($"La reserva {Codigo} ya esta cancelada.");
        Estado = EstadoReserva.Cancelada;
        FechaCancelacion = ahora;
    }

    /// <summary>Convierte "TUR-005" en el identificador numérico. Exige el prefijo.</summary>
    public static bool TryIdDesdeCodigo(string? codigo, out int id)
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(codigo)) return false;
        var texto = codigo.Trim();
        if (!texto.StartsWith(PrefijoCodigo, StringComparison.OrdinalIgnoreCase)) return false;
        return int.TryParse(texto[PrefijoCodigo.Length..], NumberStyles.None, CultureInfo.InvariantCulture, out id) && id > 0;
    }
}
