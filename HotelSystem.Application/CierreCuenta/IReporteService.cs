namespace HotelSystem.Application.CierreCuenta;

/// <summary>Reportes de gestión para el administrador (HU-13, RF-13).</summary>
public interface IReporteService
{
    /// <summary>Calcula el reporte del rango de fechas, ambas incluidas.</summary>
    ReporteGestion Generar(DateOnly desde, DateOnly hasta);

    /// <summary>Genera el reporte y lo devuelve como archivo en el formato pedido.</summary>
    ArchivoExportado Exportar(DateOnly desde, DateOnly hasta, FormatoReporte formato, int empleadoId);
}
