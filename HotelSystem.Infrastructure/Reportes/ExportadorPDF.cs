using HotelSystem.Application.CierreCuenta;
using HotelSystem.Application.Comun;

namespace HotelSystem.Infrastructure.Reportes;

/// <summary>
/// Patrón Strategy (estrategia concreta): exporta el reporte de gestión como documento PDF.
/// </summary>
public sealed class ExportadorPDF : IExportadorReporte
{
    private const double TamanoCelda = 9;
    private const double AltoFila = 14;

    public FormatoReporte FormatoSoportado => FormatoReporte.Pdf;

    public ArchivoExportado Exportar(ReporteGestion reporte)
    {
        var pdf = new EscritorPdf();

        pdf.Texto("Hotel Fast and Furious", EscritorPdf.Negrita, 18, EscritorPdf.Margen);
        pdf.Avanzar(22);
        pdf.Texto("Reporte de gestión", EscritorPdf.Normal, 13, EscritorPdf.Margen);
        pdf.Avanzar(18);
        pdf.Texto($"Periodo: {reporte.Periodo}", EscritorPdf.Normal, 10, EscritorPdf.Margen, gris: 0.4);
        pdf.Avanzar(14);

        var generado = $"{Formato.FechaConAnio(DateOnly.FromDateTime(reporte.GeneradoEn))}, {Formato.Hora(reporte.GeneradoEn)}";
        pdf.Texto($"Generado: {generado}", EscritorPdf.Normal, 10, EscritorPdf.Margen, gris: 0.4);
        pdf.Avanzar(28);

        if (reporte.SinDatos)
        {
            pdf.Texto("Sin datos para el periodo seleccionado.", EscritorPdf.Negrita, 11, EscritorPdf.Margen);
            pdf.Avanzar(26);
        }

        foreach (var seccion in reporte.ComoSecciones())
        {
            EscribirTabla(pdf, seccion);
        }

        var nombre = $"Reporte_gestion_{reporte.Desde:yyyy-MM-dd}_{reporte.Hasta:yyyy-MM-dd}.pdf";
        return new ArchivoExportado(nombre, "application/pdf", pdf.Generar("Hotel Fast and Furious"));
    }

    private static void EscribirTabla(EscritorPdf pdf, SeccionReporte seccion)
    {
        var columnas = seccion.Encabezados.Count;
        var anchoCaracter = EscritorPdf.AnchoCaracterMono * TamanoCelda;
        var caracteresPorLinea = (int)(pdf.AnchoUtil / anchoCaracter);

        // Ancho de cada columna en caracteres: su texto más largo más un margen.
        // La primera columna se queda con el espacio que sobre.
        var anchos = new int[columnas];
        var numericas = new bool[columnas];
        for (var c = 0; c < columnas; c++)
        {
            var largo = seccion.Encabezados[c].Length;
            var todasNumericas = seccion.Filas.Count > 0;
            foreach (var fila in seccion.Filas)
            {
                if (c >= fila.Count)
                {
                    todasNumericas = false;
                    continue;
                }

                largo = Math.Max(largo, fila[c].Texto.Length);
                todasNumericas = todasNumericas && fila[c].EsNumero;
            }

            anchos[c] = largo + 2;
            numericas[c] = todasNumericas;
        }

        if (columnas > 0)
        {
            anchos[0] = Math.Max(8, caracteresPorLinea - anchos.Skip(1).Sum());
        }

        pdf.AsegurarEspacio(70);
        pdf.Texto(seccion.Titulo, EscritorPdf.Negrita, 12, EscritorPdf.Margen);
        pdf.Avanzar(18);

        EscribirFila(pdf, seccion.Encabezados, anchos, numericas, anchoCaracter, EscritorPdf.MonoNegrita, 0.35);
        pdf.Linea(EscritorPdf.Margen, EscritorPdf.Margen + pdf.AnchoUtil, -4);
        pdf.Avanzar(AltoFila + 1);

        if (seccion.Filas.Count == 0)
        {
            pdf.Texto("Sin registros en el periodo.", EscritorPdf.Mono, TamanoCelda, EscritorPdf.Margen + 3, gris: 0.5);
            pdf.Avanzar(AltoFila);
        }

        foreach (var fila in seccion.Filas)
        {
            pdf.AsegurarEspacio(AltoFila + 1);
            EscribirFila(pdf, fila.Select(celda => celda.Texto).ToList(), anchos, numericas, anchoCaracter, EscritorPdf.Mono, 0.15);
            pdf.Avanzar(AltoFila);
        }

        pdf.Avanzar(14);
    }

    private static void EscribirFila(EscritorPdf pdf, IReadOnlyList<string> textos, int[] anchos, bool[] numericas, double anchoCaracter, string fuente, double gris)
    {
        var x = EscritorPdf.Margen;
        for (var c = 0; c < anchos.Length; c++)
        {
            var anchoColumna = anchos[c] * anchoCaracter;
            var texto = c < textos.Count ? textos[c] : string.Empty;

            // Si no cabe, se recorta y se marca con un punto.
            var maximo = Math.Max(2, anchos[c] - 1);
            if (texto.Length > maximo)
            {
                texto = texto[..(maximo - 1)] + ".";
            }

            if (numericas[c])
            {
                pdf.Texto(texto, fuente, TamanoCelda, x + anchoColumna - 3, alinearDerecha: true, gris: gris);
            }
            else
            {
                pdf.Texto(texto, fuente, TamanoCelda, x + 3, gris: gris);
            }

            x += anchoColumna;
        }
    }
}
