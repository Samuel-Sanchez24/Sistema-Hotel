using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Domain.Entidades;

/// <summary>
/// Consumo de un servicio adicional cargado a la cuenta de una estancia (HU-08).
/// Se crea siempre a través de <see cref="Cuenta.RegistrarConsumo"/>.
/// </summary>
public class ConsumoServicio
{
    private ConsumoServicio()
    {
    }

    internal ConsumoServicio(Cuenta cuenta, ServicioAdicional servicio, int cantidad, decimal precioAplicado, DateTime fechaHora, int? registradoPorId)
    {
        if (cantidad < 1) throw new ReglaNegocioException("La cantidad debe ser de al menos una unidad.");
        if (precioAplicado <= 0) throw new ReglaNegocioException("Ingrese un monto valido.");

        Cuenta = cuenta;
        CuentaId = cuenta.Id;
        Servicio = servicio;
        ServicioAdicionalId = servicio.Id;
        Cantidad = cantidad;
        PrecioAplicado = precioAplicado;
        FechaHora = fechaHora;
        RegistradoPorId = registradoPorId;
    }

    public int Id { get; private set; }

    public int CuentaId { get; private set; }

    public Cuenta Cuenta { get; private set; } = null!;

    public int ServicioAdicionalId { get; private set; }

    public ServicioAdicional Servicio { get; private set; } = null!;

    public int Cantidad { get; private set; }

    /// <summary>Precio unitario cobrado. Puede diferir del precio de catálogo (restaurante, bar...).</summary>
    public decimal PrecioAplicado { get; private set; }

    public DateTime FechaHora { get; private set; }

    /// <summary>Empleado que registró el consumo en el punto de servicio.</summary>
    public int? RegistradoPorId { get; private set; }

    public decimal Subtotal => Cantidad * PrecioAplicado;
}
