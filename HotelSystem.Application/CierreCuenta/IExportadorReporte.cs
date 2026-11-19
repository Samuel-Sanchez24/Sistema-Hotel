namespace HotelSystem.Application.CierreCuenta;

/// <summary>Formatos en los que se puede exportar un reporte (RF-13).</summary>
public enum FormatoReporte
{
    Pdf,
    Excel,
}

/// <summary>Archivo listo para descargar.</summary>
public sealed record ArchivoExportado(string NombreArchivo, string TipoContenido, byte[] Contenido);

/// <summary>
/// Patrón Strategy (interfaz de la estrategia). Cada formato de exportación es una clase que
/// implementa esta interfaz: <c>ExportadorPDF</c> y <c>ExportadorExcel</c>, en
/// HotelSystem.Infrastructure. <see cref="GeneradorReportes"/> es el contexto: obtiene los datos
/// y delega el formato en la estrategia que corresponda.
///
/// Para agregar un formato nuevo (por ejemplo CSV) se crea otra clase y se registra en la
/// inyección de dependencias. No se modifica <see cref="GeneradorReportes"/> ni las estrategias
/// existentes (principio abierto/cerrado).
///
/// Contrato que debe cumplir cualquier estrategia (principio de sustitución de Liskov):
/// devuelve siempre un archivo con contenido, incluso si el reporte no tiene datos; no
/// modifica el reporte recibido; y el nombre del archivo lleva la extensión de su formato.
/// </summary>
public interface IExportadorReporte
{
    /// <summary>Formato que produce esta estrategia.</summary>
    FormatoReporte FormatoSoportado { get; }

    ArchivoExportado Exportar(ReporteGestion reporte);
}
