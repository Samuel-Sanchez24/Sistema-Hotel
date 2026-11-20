namespace HotelSystem.Domain.Entidades;

/// <summary>
/// Resumen de la cuenta consolidada (HU-12). Es solo una prefactura: el sistema no emite
/// comprobantes fiscales ni calcula impuestos. Se guarda una al generarla en recepción
/// y otra, marcada como final, al cerrar la cuenta.
/// </summary>
public class Prefactura
{
    private Prefactura()
    {
    }

    public Prefactura(Cuenta cuenta, DateTime fechaGeneracion, bool esFinal)
    {
        Cuenta = cuenta;
        CuentaId = cuenta.Id;
        FechaGeneracion = fechaGeneracion;
        MontoAlojamiento = cuenta.TotalAlojamiento;
        MontoServicios = cuenta.TotalServicios;
        MontoTours = cuenta.TotalTours;
        MontoTotal = cuenta.CalcularTotal();
        EsFinal = esFinal;
    }

    public int Id { get; private set; }

    public int CuentaId { get; private set; }

    public Cuenta Cuenta { get; private set; } = null!;

    public DateTime FechaGeneracion { get; private set; }

    public decimal MontoAlojamiento { get; private set; }

    public decimal MontoServicios { get; private set; }

    public decimal MontoTours { get; private set; }

    /// <summary>Monto consolidado: alojamiento + servicios + tours (RN-07).</summary>
    public decimal MontoTotal { get; private set; }

    /// <summary>Verdadero si se generó al cerrar la cuenta.</summary>
    public bool EsFinal { get; private set; }
}
