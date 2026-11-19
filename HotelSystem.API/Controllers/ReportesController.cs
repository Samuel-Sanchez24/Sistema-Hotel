using HotelSystem.API.Seguridad;
using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.CierreCuenta;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.API.Controllers;

/// <summary>Reportes de gestion (HU-13, RF-13). Solo administrador.</summary>
[Route("api/reportes")]
[Authorize(Roles = RolesApi.Administrador)]
public sealed class ReportesController(IReporteService reportes, IReloj reloj) : ApiControllerBase
{
    /// <summary>Ocupacion, ingresos, servicios y personal del periodo. Por defecto, del dia 1 del mes hasta hoy.</summary>
    [HttpGet]
    public ReporteGestion Generar([FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
    {
        var (inicio, fin) = Periodo(desde, hasta);
        return reportes.Generar(inicio, fin);
    }

    /// <summary>
    /// Descarga el reporte como archivo (patron Strategy: el formato elige el exportador).
    /// Ejemplo: /api/reportes/exportar?formato=excel&amp;desde=2026-11-01&amp;hasta=2026-11-30
    /// </summary>
    [HttpGet("exportar")]
    public IActionResult Exportar([FromQuery] string? formato, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta)
    {
        var (inicio, fin) = Periodo(desde, hasta);
        var pideExcel = (formato ?? "pdf").Trim().ToLowerInvariant() is "excel" or "xlsx";
        var tipo = pideExcel ? FormatoReporte.Excel : FormatoReporte.Pdf;

        var archivo = reportes.Exportar(inicio, fin, tipo, UsuarioId);
        return File(archivo.Contenido, archivo.TipoContenido, archivo.NombreArchivo);
    }

    private (DateOnly Desde, DateOnly Hasta) Periodo(DateOnly? desde, DateOnly? hasta)
    {
        var hoy = reloj.Hoy;
        return (desde ?? new DateOnly(hoy.Year, hoy.Month, 1), hasta ?? hoy);
    }
}
