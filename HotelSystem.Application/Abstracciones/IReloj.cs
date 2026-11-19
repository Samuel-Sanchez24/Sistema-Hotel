namespace HotelSystem.Application.Abstracciones;

/// <summary>
/// Fuente de la fecha y hora actuales. Los servicios nunca usan <c>DateTime.Now</c>
/// directamente: así las pruebas pueden fijar "hoy" y la demostración puede anclarse
/// a una fecha (ver Hotel:FechaActual en appsettings.json).
/// </summary>
public interface IReloj
{
    DateTime Ahora { get; }

    DateOnly Hoy { get; }
}
