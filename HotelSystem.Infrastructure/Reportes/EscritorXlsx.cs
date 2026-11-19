using System.Globalization;
using System.IO.Compression;
using System.Security;
using System.Text;

namespace HotelSystem.Infrastructure.Reportes;

/// <summary>
/// Generador mínimo de libros de Excel (.xlsx), escrito solo con la biblioteca base de .NET.
/// Un .xlsx es un archivo ZIP con documentos XML: el libro, una hoja por pestaña y los estilos.
/// Los números se guardan como números (no como texto) para que Excel pueda sumarlos.
/// </summary>
internal sealed class EscritorXlsx
{
    private const int EstiloNormal = 0;
    private const int EstiloEncabezado = 1;
    private const int EstiloNumero = 2;
    private const int EstiloTitulo = 3;

    private const string RelacionesRaiz = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships"><Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="xl/workbook.xml"/></Relationships>
        """;

    // Índices de cellXfs: 0 normal, 1 encabezado (negrita con fondo gris), 2 número con
    // separador de miles, 3 título (negrita, tamaño 14).
    private const string Estilos = """
        <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
        <styleSheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">
        <fonts count="3"><font><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="11"/><name val="Calibri"/></font><font><b/><sz val="14"/><name val="Calibri"/></font></fonts>
        <fills count="3"><fill><patternFill patternType="none"/></fill><fill><patternFill patternType="gray125"/></fill><fill><patternFill patternType="solid"><fgColor rgb="FFE5E7EB"/><bgColor indexed="64"/></patternFill></fill></fills>
        <borders count="1"><border><left/><right/><top/><bottom/><diagonal/></border></borders>
        <cellStyleXfs count="1"><xf numFmtId="0" fontId="0" fillId="0" borderId="0"/></cellStyleXfs>
        <cellXfs count="4"><xf numFmtId="0" fontId="0" fillId="0" borderId="0" xfId="0"/><xf numFmtId="0" fontId="1" fillId="2" borderId="0" xfId="0" applyFont="1" applyFill="1"/><xf numFmtId="3" fontId="0" fillId="0" borderId="0" xfId="0" applyNumberFormat="1"/><xf numFmtId="0" fontId="2" fillId="0" borderId="0" xfId="0" applyFont="1"/></cellXfs>
        <cellStyles count="1"><cellStyle name="Normal" xfId="0" builtinId="0"/></cellStyles>
        </styleSheet>
        """;

    private readonly List<Hoja> _hojas = new();

    /// <summary>Celda de una hoja: texto o número, con su estilo.</summary>
    internal readonly record struct Celda(string? Texto, decimal? Valor, int Estilo);

    public static Celda Texto(string texto) => new(texto, null, EstiloNormal);

    public static Celda Encabezado(string texto) => new(texto, null, EstiloEncabezado);

    public static Celda Titulo(string texto) => new(texto, null, EstiloTitulo);

    public static Celda Numero(decimal valor) => new(null, valor, EstiloNumero);

    /// <summary>Agrega una pestaña al libro. Excel limita el nombre a 31 caracteres y prohíbe algunos símbolos.</summary>
    public Hoja AgregarHoja(string nombre)
    {
        var limpio = new string(nombre.Where(c => !"[]:*?/\\".Contains(c)).ToArray()).Trim();
        if (limpio.Length == 0) limpio = "Hoja" + (_hojas.Count + 1).ToString(CultureInfo.InvariantCulture);
        if (limpio.Length > 31) limpio = limpio[..31];

        var hoja = new Hoja(limpio);
        _hojas.Add(hoja);
        return hoja;
    }

    /// <summary>Devuelve el archivo .xlsx completo.</summary>
    public byte[] Generar()
    {
        using var memoria = new MemoryStream();
        using (var zip = new ZipArchive(memoria, ZipArchiveMode.Create, leaveOpen: true))
        {
            Agregar(zip, "[Content_Types].xml", TiposDeContenido());
            Agregar(zip, "_rels/.rels", RelacionesRaiz);
            Agregar(zip, "xl/workbook.xml", Libro());
            Agregar(zip, "xl/_rels/workbook.xml.rels", RelacionesLibro());
            Agregar(zip, "xl/styles.xml", Estilos);
            for (var i = 0; i < _hojas.Count; i++)
            {
                Agregar(zip, "xl/worksheets/sheet" + Entero(i + 1) + ".xml", XmlHoja(_hojas[i]));
            }
        }

        return memoria.ToArray();
    }

    private static void Agregar(ZipArchive zip, string ruta, string contenido)
    {
        var entrada = zip.CreateEntry(ruta, CompressionLevel.Optimal);
        using var flujo = entrada.Open();
        var bytes = new UTF8Encoding(false).GetBytes(contenido);
        flujo.Write(bytes, 0, bytes.Length);
    }

    private string TiposDeContenido()
    {
        var xml = new StringBuilder();
        xml.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""").Append('\n');
        xml.Append("""<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">""");
        xml.Append("""<Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>""");
        xml.Append("""<Default Extension="xml" ContentType="application/xml"/>""");
        xml.Append("""<Override PartName="/xl/workbook.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml"/>""");
        xml.Append("""<Override PartName="/xl/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.spreadsheetml.styles+xml"/>""");
        for (var i = 0; i < _hojas.Count; i++)
        {
            xml.Append("<Override PartName=\"/xl/worksheets/sheet").Append(Entero(i + 1))
                .Append(".xml\" ContentType=\"application/vnd.openxmlformats-officedocument.spreadsheetml.worksheet+xml\"/>");
        }

        xml.Append("</Types>");
        return xml.ToString();
    }

    private string Libro()
    {
        var xml = new StringBuilder();
        xml.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""").Append('\n');
        xml.Append("""<workbook xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main" xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships"><sheets>""");
        for (var i = 0; i < _hojas.Count; i++)
        {
            xml.Append("<sheet name=\"").Append(Xml(_hojas[i].Nombre))
                .Append("\" sheetId=\"").Append(Entero(i + 1))
                .Append("\" r:id=\"rId").Append(Entero(i + 1)).Append("\"/>");
        }

        xml.Append("</sheets></workbook>");
        return xml.ToString();
    }

    private string RelacionesLibro()
    {
        var xml = new StringBuilder();
        xml.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""").Append('\n');
        xml.Append("""<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">""");
        for (var i = 0; i < _hojas.Count; i++)
        {
            xml.Append("<Relationship Id=\"rId").Append(Entero(i + 1))
                .Append("\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet\" Target=\"worksheets/sheet")
                .Append(Entero(i + 1)).Append(".xml\"/>");
        }

        xml.Append("<Relationship Id=\"rId").Append(Entero(_hojas.Count + 1))
            .Append("\" Type=\"http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles\" Target=\"styles.xml\"/>");
        xml.Append("</Relationships>");
        return xml.ToString();
    }

    private static string XmlHoja(Hoja hoja)
    {
        var xml = new StringBuilder();
        xml.Append("""<?xml version="1.0" encoding="UTF-8" standalone="yes"?>""").Append('\n');
        xml.Append("""<worksheet xmlns="http://schemas.openxmlformats.org/spreadsheetml/2006/main">""");

        // Ancho de cada columna según su contenido más largo.
        var columnas = hoja.Filas.Count == 0 ? 0 : hoja.Filas.Max(x => x.Length);
        if (columnas > 0)
        {
            xml.Append("<cols>");
            for (var c = 0; c < columnas; c++)
            {
                var largo = 8;
                foreach (var fila in hoja.Filas)
                {
                    // El título de la hoja ocupa una sola celda larga y no debe ensanchar la columna.
                    if (c >= fila.Length || fila[c].Estilo == EstiloTitulo || fila.Length == 1) continue;
                    var texto = fila[c].Valor is { } numero ? numero.ToString("N0", CultureInfo.InvariantCulture) : fila[c].Texto ?? string.Empty;
                    largo = Math.Max(largo, texto.Length);
                }

                var ancho = Math.Min(60, largo + 3);
                xml.Append("<col min=\"").Append(Entero(c + 1)).Append("\" max=\"").Append(Entero(c + 1))
                    .Append("\" width=\"").Append(Entero(ancho)).Append("\" customWidth=\"1\"/>");
            }

            xml.Append("</cols>");
        }

        xml.Append("<sheetData>");
        for (var f = 0; f < hoja.Filas.Count; f++)
        {
            var fila = hoja.Filas[f];
            xml.Append("<row r=\"").Append(Entero(f + 1)).Append("\">");
            for (var c = 0; c < fila.Length; c++)
            {
                var celda = fila[c];
                var referencia = LetraColumna(c) + Entero(f + 1);
                if (celda.Valor is { } valor)
                {
                    xml.Append("<c r=\"").Append(referencia).Append("\" s=\"").Append(Entero(celda.Estilo)).Append("\"><v>")
                        .Append(valor.ToString(CultureInfo.InvariantCulture)).Append("</v></c>");
                }
                else if (!string.IsNullOrEmpty(celda.Texto))
                {
                    xml.Append("<c r=\"").Append(referencia).Append("\" s=\"").Append(Entero(celda.Estilo))
                        .Append("\" t=\"inlineStr\"><is><t xml:space=\"preserve\">").Append(Xml(celda.Texto)).Append("</t></is></c>");
                }
            }

            xml.Append("</row>");
        }

        xml.Append("</sheetData></worksheet>");
        return xml.ToString();
    }

    /// <summary>0 -> A, 1 -> B, ... 25 -> Z, 26 -> AA.</summary>
    private static string LetraColumna(int indice)
    {
        var letras = string.Empty;
        var numero = indice + 1;
        while (numero > 0)
        {
            var resto = (numero - 1) % 26;
            letras = (char)('A' + resto) + letras;
            numero = (numero - 1) / 26;
        }

        return letras;
    }

    private static string Entero(int valor) => valor.ToString(CultureInfo.InvariantCulture);

    private static string Xml(string texto) => SecurityElement.Escape(texto) ?? string.Empty;

    /// <summary>Pestaña del libro: una lista de filas, y cada fila una lista de celdas.</summary>
    internal sealed class Hoja(string nombre)
    {
        public string Nombre { get; } = nombre;

        public List<Celda[]> Filas { get; } = new();

        public void Fila(params Celda[] celdas) => Filas.Add(celdas);
    }
}
