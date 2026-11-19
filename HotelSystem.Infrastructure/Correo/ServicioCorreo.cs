using System.Net;
using System.Net.Mail;
using System.Text;
using HotelSystem.Application.Abstracciones;
using Microsoft.Extensions.Logging;

namespace HotelSystem.Infrastructure.Correo;

/// <summary>Configuración del correo. Se lee de la sección "Correo" de appsettings.json.</summary>
public sealed class OpcionesCorreo
{
    /// <summary>
    /// "Carpeta": guarda cada correo como archivo HTML en <see cref="CarpetaSalida"/> (desarrollo y demostración).
    /// "Smtp": lo envía de verdad por el servidor configurado.
    /// "Desactivado": no envía nada.
    /// </summary>
    public string Modo { get; set; } = "Carpeta";

    public string CarpetaSalida { get; set; } = string.Empty;

    public string Servidor { get; set; } = string.Empty;

    public int Puerto { get; set; } = 587;

    public bool UsarSsl { get; set; } = true;

    public string Usuario { get; set; } = string.Empty;

    public string Clave { get; set; } = string.Empty;

    public string Remitente { get; set; } = "reservas@hotelfastandfurious.com";

    public string NombreRemitente { get; set; } = "Hotel Fast and Furious";
}

/// <summary>
/// Envío de correo (HU-04). Es el único punto del sistema que habla con el servicio de
/// correo externo del diagrama de contexto (C4 nivel 1).
/// </summary>
public sealed class ServicioCorreo(OpcionesCorreo opciones, ILogger<ServicioCorreo> logger) : IServicioCorreo
{
    public bool Enviar(MensajeCorreo mensaje)
    {
        try
        {
            switch ((opciones.Modo ?? string.Empty).Trim().ToLowerInvariant())
            {
                case "smtp":
                    EnviarPorSmtp(mensaje);
                    return true;

                case "carpeta":
                    GuardarEnCarpeta(mensaje);
                    return true;

                default:
                    logger.LogInformation("Correo desactivado. No se envio \"{Asunto}\" a {Destinatario}.", mensaje.Asunto, mensaje.Para);
                    return false;
            }
        }
        catch (Exception ex)
        {
            // Un fallo de correo nunca debe tumbar la operación que lo originó.
            logger.LogError(ex, "No se pudo enviar el correo \"{Asunto}\" a {Destinatario}.", mensaje.Asunto, mensaje.Para);
            return false;
        }
    }

    private void EnviarPorSmtp(MensajeCorreo mensaje)
    {
        using var correo = new MailMessage
        {
            From = new MailAddress(opciones.Remitente, opciones.NombreRemitente),
            Subject = mensaje.Asunto,
            SubjectEncoding = Encoding.UTF8,
            Body = mensaje.CuerpoHtml,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true,
        };
        correo.To.Add(new MailAddress(mensaje.Para, mensaje.NombreDestinatario));

        using var cliente = new SmtpClient(opciones.Servidor, opciones.Puerto)
        {
            EnableSsl = opciones.UsarSsl,
            Timeout = 15000,
        };
        if (!string.IsNullOrWhiteSpace(opciones.Usuario))
        {
            cliente.Credentials = new NetworkCredential(opciones.Usuario, opciones.Clave);
        }

        cliente.Send(correo);
        logger.LogInformation("Correo enviado a {Destinatario}: \"{Asunto}\".", mensaje.Para, mensaje.Asunto);
    }

    /// <summary>
    /// Guarda el correo como una página HTML que se abre con doble clic: arriba muestra
    /// destinatario y asunto, y debajo el correo tal como lo vería el huésped.
    /// </summary>
    private void GuardarEnCarpeta(MensajeCorreo mensaje)
    {
        var carpeta = string.IsNullOrWhiteSpace(opciones.CarpetaSalida)
            ? Path.Combine(AppContext.BaseDirectory, "correos")
            : opciones.CarpetaSalida;
        Directory.CreateDirectory(carpeta);

        var ahora = DateTime.Now;
        var destinatario = new string(mensaje.Para.Select(c => char.IsLetterOrDigit(c) || c is '.' or '@' or '-' or '_' ? c : '_').ToArray());
        var ruta = Path.Combine(carpeta, $"{ahora:yyyyMMdd-HHmmss-fff}_{destinatario}.html");

        var pagina = new StringBuilder();
        pagina.AppendLine("<!DOCTYPE html>");
        pagina.AppendLine($"<html lang=\"es\"><head><meta charset=\"utf-8\"><title>{WebUtility.HtmlEncode(mensaje.Asunto)}</title></head>");
        pagina.AppendLine("<body style=\"margin:0;background:#111827\">");
        pagina.AppendLine("<div style=\"font-family:Consolas,monospace;font-size:12px;color:#e5e7eb;padding:12px 16px;line-height:1.6\">");
        pagina.AppendLine($"<div>Para: {WebUtility.HtmlEncode(mensaje.NombreDestinatario)} &lt;{WebUtility.HtmlEncode(mensaje.Para)}&gt;</div>");
        pagina.AppendLine($"<div>Asunto: {WebUtility.HtmlEncode(mensaje.Asunto)}</div>");
        pagina.AppendLine($"<div>Fecha: {ahora:yyyy-MM-dd HH:mm:ss} - modo Carpeta: este correo se guardó aquí y no salió a Internet.</div>");
        pagina.AppendLine("</div>");
        pagina.AppendLine($"<iframe title=\"Correo\" style=\"width:100%;height:calc(100vh - 90px);border:0;background:#f3f4f6\" srcdoc=\"{WebUtility.HtmlEncode(mensaje.CuerpoHtml)}\"></iframe>");
        pagina.AppendLine("</body></html>");

        File.WriteAllText(ruta, pagina.ToString(), new UTF8Encoding(false));
        logger.LogInformation("Correo guardado en {Ruta}. Para: {Destinatario}. Asunto: \"{Asunto}\".", ruta, mensaje.Para, mensaje.Asunto);
    }
}
