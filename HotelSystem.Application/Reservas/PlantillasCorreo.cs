using System.Net;
using System.Text;
using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.Comun;
using HotelSystem.Domain.Entidades;

namespace HotelSystem.Application.Reservas;

/// <summary>Textos de los correos que envía el sistema.</summary>
public static class PlantillasCorreo
{
    /// <summary>Confirmación automática de una reserva de habitación (HU-04, RF-02).</summary>
    public static MensajeCorreo ConfirmacionReserva(ReservaHabitacion reserva, DateTime limiteCancelacionSinCargo)
    {
        var huesped = reserva.Huesped;
        var habitacion = reserva.Habitacion;
        var limite = $"{Formato.FechaLarga(DateOnly.FromDateTime(limiteCancelacionSinCargo))} a las {Formato.Hora(limiteCancelacionSinCargo)}";

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"es\"><head><meta charset=\"utf-8\"><title>Confirmación de reserva</title></head>");
        html.AppendLine("<body style=\"font-family:Arial,Helvetica,sans-serif;color:#1f2937;background:#f3f4f6;padding:24px\">");
        html.AppendLine("<div style=\"max-width:560px;margin:0 auto;background:#ffffff;border-radius:12px;padding:28px\">");
        html.AppendLine("<h1 style=\"font-size:20px;margin:0 0 4px 0\">Hotel Fast and Furious</h1>");
        html.AppendLine($"<p style=\"color:#6b7280;margin:0 0 20px 0\">Confirmación de reserva {Codificar(reserva.Codigo)}</p>");
        html.AppendLine($"<p>Hola, {Codificar(huesped.Nombre)}:</p>");
        html.AppendLine("<p>Tu reserva quedó confirmada. Estos son los datos:</p>");
        html.AppendLine("<table style=\"width:100%;border-collapse:collapse;font-size:14px\">");
        Fila(html, "Reserva", reserva.Codigo);
        Fila(html, "Habitación", $"{habitacion.Numero} ({habitacion.Tipo}), piso {habitacion.Piso}");
        Fila(html, "Entrada", Formato.FechaLarga(reserva.FechaEntrada));
        Fila(html, "Salida", Formato.FechaLarga(reserva.FechaSalida));
        Fila(html, "Noches", reserva.Noches.ToString());
        Fila(html, "Huéspedes", reserva.CantidadHuespedes.ToString());
        Fila(html, "Tarifa por noche", Formato.Dinero(reserva.PrecioNoche));
        Fila(html, "Total del alojamiento", Formato.Dinero(reserva.ValorTotalAlojamiento));
        html.AppendLine("</table>");
        html.AppendLine($"<p style=\"font-size:13px;color:#6b7280;margin-top:20px\">Puedes cancelar sin costo hasta el {Codificar(limite)}. Después de ese momento se cobra una noche.</p>");
        html.AppendLine("<p style=\"font-size:13px;color:#6b7280\">El pago del alojamiento y de los consumos se hace en recepción al momento del check-out.</p>");
        html.AppendLine("</div></body></html>");

        return new MensajeCorreo(
            huesped.Correo,
            huesped.Nombre,
            $"Confirmación de reserva {reserva.Codigo} - Hotel Fast and Furious",
            html.ToString());
    }

    private static void Fila(StringBuilder html, string etiqueta, string valor) =>
        html.AppendLine($"<tr><td style=\"padding:8px 0;color:#6b7280;border-bottom:1px solid #e5e7eb\">{Codificar(etiqueta)}</td><td style=\"padding:8px 0;text-align:right;border-bottom:1px solid #e5e7eb\">{Codificar(valor)}</td></tr>");

    private static string Codificar(string texto) => WebUtility.HtmlEncode(texto);
}
