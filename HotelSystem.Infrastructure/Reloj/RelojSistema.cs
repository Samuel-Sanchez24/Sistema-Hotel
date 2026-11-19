using System.Globalization;
using HotelSystem.Application.Abstracciones;

namespace HotelSystem.Infrastructure.Reloj;

public sealed class OpcionesReloj
{
    /// <summary>
    /// Si tiene valor, el sistema trabaja como si hoy fuera esa fecha (la hora sigue siendo
    /// la real). Sirve para repetir una demostración con las mismas fechas.
    /// </summary>
    public DateOnly? FechaFija { get; set; }

    /// <summary>Lee la fecha en formato AAAA-MM-DD. Un texto vacío o inválido deja el reloj normal.</summary>
    public static OpcionesReloj Desde(string? fechaActual)
    {
        var opciones = new OpcionesReloj();
        if (DateOnly.TryParseExact(fechaActual, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
        {
            opciones.FechaFija = fecha;
        }

        return opciones;
    }
}

/// <summary>Reloj real del servidor, con la opción de anclar la fecha.</summary>
public sealed class RelojSistema(OpcionesReloj opciones) : IReloj
{
    public DateTime Ahora
    {
        get
        {
            var ahora = DateTime.Now;
            return opciones.FechaFija is { } fecha
                ? fecha.ToDateTime(TimeOnly.FromDateTime(ahora))
                : ahora;
        }
    }

    public DateOnly Hoy => DateOnly.FromDateTime(Ahora);
}
