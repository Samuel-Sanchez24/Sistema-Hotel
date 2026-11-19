using System.IO.Compression;
using System.Text;
using HotelSystem.Application.CierreCuenta;
using HotelSystem.Tests.Soporte;

namespace HotelSystem.Tests;

/// <summary>Reportes de gestion (HU-13, RF-13) y patron Strategy para exportar.</summary>
public class ReportesTests : IDisposable
{
    private readonly EntornoPruebas _entorno = new();

    public void Dispose() => _entorno.Dispose();

    [Fact]
    public void Reporte_DelMes_TieneDatosReales()
    {
        var reporte = _entorno.En<IReporteService, ReporteGestion>(r => r.Generar(_entorno.Hoy.AddDays(-10), _entorno.Hoy));

        Assert.False(reporte.SinDatos);
        Assert.Equal(60, reporte.TotalHabitaciones);
        Assert.Equal(13, reporte.HabitacionesOcupadasHoy);
        Assert.True(reporte.NochesOcupadas > 0);
        Assert.True(reporte.TotalIngresos > 0);
        Assert.NotEmpty(reporte.Servicios);
        Assert.NotEmpty(reporte.Personal);
    }

    [Fact]
    public void Reporte_DeUnPeriodoSinMovimiento_IndicaSinDatos()
    {
        var reporte = _entorno.En<IReporteService, ReporteGestion>(r => r.Generar(new DateOnly(2020, 1, 1), new DateOnly(2020, 1, 31)));

        Assert.True(reporte.SinDatos);
    }

    [Fact]
    public void Exportar_Pdf_GeneraUnPdfValido()
    {
        var archivo = Exportar(FormatoReporte.Pdf);

        Assert.EndsWith(".pdf", archivo.NombreArchivo);
        Assert.Equal("application/pdf", archivo.TipoContenido);
        Assert.Equal("%PDF-", Encoding.ASCII.GetString(archivo.Contenido, 0, 5));
        Assert.Contains("%%EOF", Encoding.ASCII.GetString(archivo.Contenido[^32..]));
    }

    [Fact]
    public void Exportar_Excel_GeneraUnLibroXlsxValido()
    {
        var archivo = Exportar(FormatoReporte.Excel);

        Assert.EndsWith(".xlsx", archivo.NombreArchivo);
        using var zip = new ZipArchive(new MemoryStream(archivo.Contenido), ZipArchiveMode.Read);
        var entradas = zip.Entries.Select(e => e.FullName).ToList();
        Assert.Contains("[Content_Types].xml", entradas);
        Assert.Contains("xl/workbook.xml", entradas);
        Assert.Contains(entradas, e => e.StartsWith("xl/worksheets/sheet"));
    }

    /// <summary>
    /// Sustitucion de Liskov: cualquier estrategia registrada cumple el mismo contrato,
    /// incluso con un reporte vacio.
    /// </summary>
    [Fact]
    public void Strategy_TodasLasEstrategiasCumplenElContrato()
    {
        var estrategias = _entorno.Unico<IEnumerable<IExportadorReporte>>().ToList();
        Assert.Equal(Enum.GetValues<FormatoReporte>().Length, estrategias.Select(e => e.FormatoSoportado).Distinct().Count());

        var vacio = new ReporteGestion(_entorno.Hoy, _entorno.Hoy, EntornoPruebas.Ahora, true, 0, 0, 0, [], [], [], []);
        foreach (var estrategia in estrategias)
        {
            var archivo = estrategia.Exportar(vacio);

            Assert.NotEmpty(archivo.Contenido);
            var extension = estrategia.FormatoSoportado == FormatoReporte.Pdf ? ".pdf" : ".xlsx";
            Assert.EndsWith(extension, archivo.NombreArchivo);
        }
    }

    private ArchivoExportado Exportar(FormatoReporte formato) =>
        _entorno.En<IReporteService, ArchivoExportado>(r => r.Exportar(_entorno.Hoy.AddDays(-10), _entorno.Hoy, formato, _entorno.IdPersona("rsilva")));
}
