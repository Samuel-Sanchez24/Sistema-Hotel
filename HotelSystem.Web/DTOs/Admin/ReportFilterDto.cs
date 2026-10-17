namespace HotelSystem.Web.DTOs.Admin;

/// <summary>Filtros de los reportes; tambien se usa para exportar (PDF / Excel).</summary>
public class ReportFilterDto
{
    public string Tab { get; set; } = "ocupacion";
    public string DateFrom { get; set; } = "Sep 2026";
    public string DateTo { get; set; } = "Sep 2026";
    public bool NoData { get; set; }
    public string? Format { get; set; }
}
