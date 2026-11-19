namespace HotelSystem.Application.Abstracciones;

/// <summary>Correo electrónico que el sistema envía a un usuario.</summary>
public sealed record MensajeCorreo(string Para, string NombreDestinatario, string Asunto, string CuerpoHtml);

/// <summary>
/// Envío de correo (HU-04). La implementación está en HotelSystem.Infrastructure:
/// SMTP real o, en desarrollo, archivos en una carpeta.
/// </summary>
public interface IServicioCorreo
{
    /// <summary>
    /// Devuelve verdadero si el mensaje se entregó. Nunca lanza excepción: un fallo de
    /// correo no debe deshacer una reserva que ya quedó confirmada.
    /// </summary>
    bool Enviar(MensajeCorreo mensaje);
}
