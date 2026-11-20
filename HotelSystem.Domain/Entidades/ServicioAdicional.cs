using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Domain.Entidades;

/// <summary>Servicio del catálogo del hotel: restaurante, spa, lavandería, transporte...</summary>
public class ServicioAdicional
{
    private ServicioAdicional()
    {
        Nombre = string.Empty;
        Categoria = string.Empty;
        Descripcion = string.Empty;
    }

    public ServicioAdicional(string nombre, string categoria, string descripcion, decimal precioUnitario, bool activo = true)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ReglaNegocioException("El nombre del servicio es obligatorio.");
        if (precioUnitario < 0) throw new ReglaNegocioException("El precio no puede ser negativo.");

        Nombre = nombre.Trim();
        Categoria = categoria.Trim();
        Descripcion = descripcion.Trim();
        PrecioUnitario = precioUnitario;
        Activo = activo;
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; }

    public string Categoria { get; private set; }

    public string Descripcion { get; private set; }

    /// <summary>Precio de catálogo. Cero significa precio variable según el consumo.</summary>
    public decimal PrecioUnitario { get; private set; }

    public bool Activo { get; private set; }

    public bool TienePrecioFijo => PrecioUnitario > 0;
}
