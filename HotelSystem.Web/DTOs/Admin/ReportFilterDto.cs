namespace HotelSystem.Web.DTOs.Admin;

/// <summary>Filtros de los reportes; tambien se usa para exportar (PDF / Excel).</summary>
public class ReportFilterDto
{
    public string Tab { get; set; } = "ocupacion";

    /// <summary>Inicio del periodo en formato AAAA-MM-DD. Vacio = primer dia del mes actual.</summary>
    public string DateFrom { get; set; } = "";

    /// <summary>Fin del periodo en formato AAAA-MM-DD. Vacio = hoy.</summary>
    public string DateTo { get; set; } = "";

    /// <summary>Lo calcula el servidor: no hay ningun registro en el periodo (HU-13 Esc.2).</summary>
    public bool NoData { get; set; }
    public string? Format { get; set; }
}
