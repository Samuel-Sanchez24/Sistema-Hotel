using System.Globalization;

namespace HotelSystem.Application.Comun;

/// <summary>Formatos de dinero y fechas usados en mensajes, correos y reportes.</summary>
public static class Formato
{
    private static readonly CultureInfo Colombia = CultureInfo.GetCultureInfo("es-CO");

    private static readonly string[] MesesCortos =
        ["Ene", "Feb", "Mar", "Abr", "May", "Jun", "Jul", "Ago", "Sep", "Oct", "Nov", "Dic"];

    private static readonly string[] MesesLargos =
        ["enero", "febrero", "marzo", "abril", "mayo", "junio", "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"];

    /// <summary>Pesos sin decimales: $1.250.000</summary>
    public static string Dinero(decimal monto) => "$" + monto.ToString("N0", Colombia);

    /// <summary>9 Sep</summary>
    public static string FechaCorta(DateOnly fecha) => $"{fecha.Day} {MesesCortos[fecha.Month - 1]}";

    /// <summary>9 Sep 2026</summary>
    public static string FechaConAnio(DateOnly fecha) => $"{fecha.Day} {MesesCortos[fecha.Month - 1]} {fecha.Year}";

    /// <summary>9 de septiembre de 2026</summary>
    public static string FechaLarga(DateOnly fecha) => $"{fecha.Day} de {MesesLargos[fecha.Month - 1]} de {fecha.Year}";

    /// <summary>Hora en formato de 24 horas: 14:05</summary>
    public static string Hora(DateTime momento) => momento.ToString("HH:mm", CultureInfo.InvariantCulture);

    /// <summary>Hora en formato de 12 horas: 8:00 AM</summary>
    public static string Hora12(TimeOnly hora) => hora.ToString("h:mm tt", CultureInfo.InvariantCulture);
}
