using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Domain.Entidades;

/// <summary>
/// Entrada de la bitácora: qué empleado hizo qué operación y cuándo.
/// Es la fuente del reporte de desempeño del personal (HU-13).
/// </summary>
public class RegistroOperacion
{
    private RegistroOperacion()
    {
        Detalle = string.Empty;
    }

    public RegistroOperacion(int empleadoId, TipoOperacion tipo, string detalle, DateTime fechaHora)
    {
        EmpleadoId = empleadoId;
        Tipo = tipo;
        Detalle = detalle.Length > 300 ? detalle[..300] : detalle;
        FechaHora = fechaHora;
    }

    public int Id { get; private set; }

    public int EmpleadoId { get; private set; }

    public TipoOperacion Tipo { get; private set; }

    public string Detalle { get; private set; }

    public DateTime FechaHora { get; private set; }
}
