using System.Globalization;
using System.Text;

namespace HotelSystem.Infrastructure.Reportes;

/// <summary>
/// Generador mínimo de documentos PDF (texto y líneas en páginas A4), escrito solo con la
/// biblioteca base de .NET para no depender de paquetes externos. Usa las fuentes estándar
/// de PDF, que todos los lectores traen incorporadas.
///
/// El llamador escribe de arriba hacia abajo: coloca un texto en la posición vertical
/// actual y luego avanza. Cuando no queda espacio se abre una página nueva.
/// </summary>
internal sealed class EscritorPdf
{
    public const double Ancho = 595.28;
    public const double Alto = 841.89;
    public const double Margen = 50;

    public const string Normal = "F1";
    public const string Negrita = "F2";
    public const string Mono = "F3";
    public const string MonoNegrita = "F4";

    /// <summary>En Courier todos los caracteres miden lo mismo: 0,6 veces el tamaño de la letra.</summary>
    public const double AnchoCaracterMono = 0.6;

    private static readonly string[] NombresFuente = ["Helvetica", "Helvetica-Bold", "Courier", "Courier-Bold"];

    private readonly List<StringBuilder> _paginas = new();
    private StringBuilder _actual = new();
    private double _y;

    public EscritorPdf()
    {
        NuevaPagina();
    }

    public double AnchoUtil => Ancho - (2 * Margen);

    public void NuevaPagina()
    {
        _actual = new StringBuilder();
        _paginas.Add(_actual);
        _y = Alto - Margen;
    }

    /// <summary>Abre una página nueva si lo que se va a escribir no cabe en la actual.</summary>
    public void AsegurarEspacio(double alto)
    {
        if (_y - alto < Margen + 20)
        {
            NuevaPagina();
        }
    }

    public void Avanzar(double alto) => _y -= alto;

    /// <summary>
    /// Escribe una línea de texto en la posición vertical actual. La alineación a la derecha
    /// solo es exacta con las fuentes <see cref="Mono"/> y <see cref="MonoNegrita"/>.
    /// </summary>
    public void Texto(string texto, string fuente, double tamano, double x, bool alinearDerecha = false, double gris = 0.15)
    {
        if (alinearDerecha)
        {
            x -= AnchoCaracterMono * tamano * texto.Length;
        }

        _actual.Append("BT ")
            .Append(N(gris)).Append(' ').Append(N(gris)).Append(' ').Append(N(gris)).Append(" rg /")
            .Append(fuente).Append(' ').Append(N(tamano)).Append(" Tf 1 0 0 1 ")
            .Append(N(x)).Append(' ').Append(N(_y)).Append(" Tm (")
            .Append(Escapar(texto)).Append(") Tj ET\n");
    }

    /// <summary>Línea horizontal a la altura actual más el desplazamiento indicado.</summary>
    public void Linea(double x1, double x2, double desplazamientoY = 0, double grosor = 0.5, double gris = 0.75)
    {
        var y = _y + desplazamientoY;
        _actual.Append(N(gris)).Append(' ').Append(N(gris)).Append(' ').Append(N(gris)).Append(" RG ")
            .Append(N(grosor)).Append(" w ")
            .Append(N(x1)).Append(' ').Append(N(y)).Append(" m ")
            .Append(N(x2)).Append(' ').Append(N(y)).Append(" l S\n");
    }

    /// <summary>Devuelve el archivo PDF completo. Agrega el pie con el número de página.</summary>
    public byte[] Generar(string pie)
    {
        var latin1 = Encoding.Latin1;
        var total = _paginas.Count;

        for (var i = 0; i < total; i++)
        {
            _paginas[i].Append("BT 0.45 0.45 0.45 rg /F1 8 Tf 1 0 0 1 ")
                .Append(N(Margen)).Append(" 30 Tm (")
                .Append(Escapar($"{pie} - Página {i + 1} de {total}"))
                .Append(") Tj ET\n");
        }

        // Objetos del documento, en orden: 1 catálogo, 2 árbol de páginas, 3 a 6 fuentes,
        // y por cada página su objeto de página (7, 9, 11...) y su contenido (8, 10, 12...).
        var objetos = new List<byte[]>();
        objetos.Add(latin1.GetBytes("<< /Type /Catalog /Pages 2 0 R >>"));

        var hijos = string.Join(" ", Enumerable.Range(0, total).Select(i => Entero(7 + (2 * i)) + " 0 R"));
        objetos.Add(latin1.GetBytes("<< /Type /Pages /Kids [ " + hijos + " ] /Count " + Entero(total) + " >>"));

        foreach (var fuente in NombresFuente)
        {
            objetos.Add(latin1.GetBytes("<< /Type /Font /Subtype /Type1 /BaseFont /" + fuente + " /Encoding /WinAnsiEncoding >>"));
        }

        for (var i = 0; i < total; i++)
        {
            objetos.Add(latin1.GetBytes(
                "<< /Type /Page /Parent 2 0 R /MediaBox [ 0 0 " + N(Ancho) + " " + N(Alto) + " ]"
                + " /Resources << /Font << /F1 3 0 R /F2 4 0 R /F3 5 0 R /F4 6 0 R >> >>"
                + " /Contents " + Entero(8 + (2 * i)) + " 0 R >>"));

            var contenido = latin1.GetBytes(_paginas[i].ToString());
            var cabecera = latin1.GetBytes("<< /Length " + Entero(contenido.Length) + " >>\nstream\n");
            var cierre = latin1.GetBytes("\nendstream");

            var flujo = new byte[cabecera.Length + contenido.Length + cierre.Length];
            Buffer.BlockCopy(cabecera, 0, flujo, 0, cabecera.Length);
            Buffer.BlockCopy(contenido, 0, flujo, cabecera.Length, contenido.Length);
            Buffer.BlockCopy(cierre, 0, flujo, cabecera.Length + contenido.Length, cierre.Length);
            objetos.Add(flujo);
        }

        using var salida = new MemoryStream();
        Escribir(salida, latin1.GetBytes("%PDF-1.4\n"));

        // Comentario con bytes altos: indica a los lectores que el archivo es binario.
        Escribir(salida, new byte[] { 0x25, 0xE2, 0xE3, 0xCF, 0xD3, 0x0A });

        var posiciones = new List<long>();
        for (var i = 0; i < objetos.Count; i++)
        {
            posiciones.Add(salida.Position);
            Escribir(salida, latin1.GetBytes(Entero(i + 1) + " 0 obj\n"));
            Escribir(salida, objetos[i]);
            Escribir(salida, latin1.GetBytes("\nendobj\n"));
        }

        // Tabla de referencias: cada entrada ocupa exactamente 20 bytes.
        var inicioTabla = salida.Position;
        var tabla = new StringBuilder();
        tabla.Append("xref\n0 ").Append(Entero(objetos.Count + 1)).Append('\n');
        tabla.Append("0000000000 65535 f \n");
        foreach (var posicion in posiciones)
        {
            tabla.Append(posicion.ToString("D10", CultureInfo.InvariantCulture)).Append(" 00000 n \n");
        }

        tabla.Append("trailer\n<< /Size ").Append(Entero(objetos.Count + 1)).Append(" /Root 1 0 R >>\n");
        tabla.Append("startxref\n").Append(inicioTabla.ToString(CultureInfo.InvariantCulture)).Append("\n%%EOF\n");
        Escribir(salida, latin1.GetBytes(tabla.ToString()));

        return salida.ToArray();
    }

    private static void Escribir(Stream salida, byte[] bytes) => salida.Write(bytes, 0, bytes.Length);

    /// <summary>Número con punto decimal, sin importar el idioma del sistema (PDF no acepta comas).</summary>
    private static string N(double valor) => valor.ToString("0.##", CultureInfo.InvariantCulture);

    private static string Entero(int valor) => valor.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Prepara un texto para una cadena PDF: escapa paréntesis y barras, y sustituye lo que
    /// no existe en la codificación WinAnsi (las letras con tilde y la ñ sí existen).
    /// </summary>
    private static string Escapar(string texto)
    {
        var resultado = new StringBuilder(texto.Length + 8);
        foreach (var c in texto)
        {
            switch (c)
            {
                case '\\':
                case '(':
                case ')':
                    resultado.Append('\\').Append(c);
                    break;
                case '–':
                case '—':
                    resultado.Append('-');
                    break;
                default:
                    if (c < ' ') resultado.Append(' ');
                    else if (c > 'ÿ') resultado.Append('?');
                    else resultado.Append(c);
                    break;
            }
        }

        return resultado.ToString();
    }
}
