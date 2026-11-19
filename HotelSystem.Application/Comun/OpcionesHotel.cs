using System.Globalization;

namespace HotelSystem.Application.Comun;

/// <summary>
/// Políticas del hotel que pueden cambiar sin tocar el código. Los valores se leen de la
/// sección "Hotel" de appsettings.json en cada proyecto de presentación.
/// </summary>
public sealed class OpcionesHotel
{
    /// <summary>Hora a partir de la cual se entrega la habitación. Referencia para la RN-09.</summary>
    public TimeOnly HoraCheckIn { get; set; } = new(15, 0);

    /// <summary>RN-09: horas antes del check-in hasta las que cancelar no tiene cargo.</summary>
    public int HorasCancelacionSinCargo { get; set; } = 48;

    /// <summary>Horas mínimas de anticipación para cancelar un tour.</summary>
    public int HorasCancelacionTour { get; set; } = 24;

    /// <summary>
    /// Crea las opciones a partir de los textos de configuración. Lo que falte o no se pueda
    /// interpretar conserva su valor por defecto.
    /// </summary>
    public static OpcionesHotel Desde(string? horaCheckIn, string? horasCancelacionSinCargo, string? horasCancelacionTour)
    {
        var opciones = new OpcionesHotel();

        if (TimeOnly.TryParseExact(horaCheckIn, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var hora))
        {
            opciones.HoraCheckIn = hora;
        }

        if (int.TryParse(horasCancelacionSinCargo, NumberStyles.None, CultureInfo.InvariantCulture, out var sinCargo))
        {
            opciones.HorasCancelacionSinCargo = sinCargo;
        }

        if (int.TryParse(horasCancelacionTour, NumberStyles.None, CultureInfo.InvariantCulture, out var tour))
        {
            opciones.HorasCancelacionTour = tour;
        }

        return opciones;
    }
}
