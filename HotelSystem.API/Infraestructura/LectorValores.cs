using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.API.Infraestructura;

/// <summary>
/// Lee los valores de texto que envia el front. Acepta varias escrituras del mismo valor
/// ("EN_LIMPIEZA", "EnLimpieza", "en limpieza") para que el contrato sea tolerante.
/// </summary>
public static class LectorValores
{
    public static T? LeerEnum<T>(string? texto) where T : struct, System.Enum
    {
        if (string.IsNullOrWhiteSpace(texto)) return null;

        var normalizado = texto.Replace("_", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty).Trim();
        return System.Enum.TryParse<T>(normalizado, ignoreCase: true, out var valor) && System.Enum.IsDefined(valor)
            ? valor
            : null;
    }

    /// <summary>Lee un enum obligatorio; si no es valido responde 400 con la lista de valores permitidos.</summary>
    public static T EnumRequerido<T>(string? texto, string campo) where T : struct, System.Enum =>
        LeerEnum<T>(texto) ?? throw new ReglaNegocioException(
            $"Valor no valido para {campo}. Valores permitidos: {string.Join(", ", System.Enum.GetNames<T>())}.");

    /// <summary>Rol del personal. Ademas de los nombres del enum acepta "COORDINADOR" y "ADMIN".</summary>
    public static RolEmpleado Rol(string? texto)
    {
        var normalizado = (texto ?? string.Empty).Replace("_", string.Empty).Trim().ToUpperInvariant();
        return normalizado switch
        {
            "COORDINADOR" => RolEmpleado.CoordinadorTours,
            "ADMIN" => RolEmpleado.Administrador,
            _ => EnumRequerido<RolEmpleado>(texto, "rol"),
        };
    }

    /// <summary>
    /// Respaldo de pago (RN-04). Acepta un valor de <see cref="TipoPago"/> o las opciones del
    /// prototipo ("visa", "mc", "transfer", "efectivo"). Vacio devuelve nulo y el servicio
    /// responde con el mensaje de la regla.
    /// </summary>
    public static (TipoPago? Tipo, string? Detalle) MedioPago(string? texto, string? detalle)
    {
        if (string.IsNullOrWhiteSpace(texto)) return (null, detalle);

        if (LeerEnum<TipoPago>(texto) is { } tipo) return (tipo, detalle);

        var minusculas = texto.Trim().ToLowerInvariant();
        if (minusculas.Contains("visa") || minusculas.Contains("master") || minusculas == "mc" || minusculas.Contains("tarjeta") || minusculas.Contains("datafono"))
            return (TipoPago.Tarjeta, detalle ?? texto.Trim());
        if (minusculas.Contains("transfer"))
            return (TipoPago.Transferencia, detalle ?? "Transferencia bancaria");
        if (minusculas.Contains("efectivo"))
            return (TipoPago.Efectivo, detalle ?? "Efectivo");

        return (TipoPago.Deposito, detalle ?? texto.Trim());
    }
}
