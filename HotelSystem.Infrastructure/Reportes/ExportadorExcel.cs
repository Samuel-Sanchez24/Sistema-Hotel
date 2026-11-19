using HotelSystem.Application.CierreCuenta;

namespace HotelSystem.Infrastructure.Reportes;

/// <summary>
/// Patrón Strategy (estrategia concreta): exporta el reporte de gestión como libro de Excel,
/// con una pestaña por sección (ocupación, ingresos, servicios y personal).
/// </summary>
public sealed class ExportadorExcel : IExportadorReporte
{
    public const string TipoContenido = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public FormatoReporte FormatoSoportado => FormatoReporte.Excel;

    public ArchivoExportado Exportar(ReporteGestion reporte)
    {
        var libro = new EscritorXlsx();

        foreach (var seccion in reporte.ComoSecciones())
        {
            var hoja = libro.AgregarHoja(seccion.Titulo);

            hoja.Fila(EscritorXlsx.Titulo($"Hotel Fast and Furious - {seccion.Titulo}"));
            hoja.Fila(EscritorXlsx.Texto($"Periodo: {reporte.Periodo}"));
            if (reporte.SinDatos)
            {
                hoja.Fila(EscritorXlsx.Texto("Sin datos para el periodo seleccionado."));
            }

            hoja.Fila();
            hoja.Fila(seccion.Encabezados.Select(EscritorXlsx.Encabezado).ToArray());

            foreach (var fila in seccion.Filas)
            {
                hoja.Fila(fila
                    .Select(celda => celda.Valor is { } valor ? EscritorXlsx.Numero(valor) : EscritorXlsx.Texto(celda.Texto))
                    .ToArray());
            }
        }

        var nombre = $"Reporte_gestion_{reporte.Desde:yyyy-MM-dd}_{reporte.Hasta:yyyy-MM-dd}.xlsx";
        return new ArchivoExportado(nombre, TipoContenido, libro.Generar());
    }
}
