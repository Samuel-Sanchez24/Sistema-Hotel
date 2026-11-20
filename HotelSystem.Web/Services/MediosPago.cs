using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Web.Services;

/// <summary>
/// Convierte el valor elegido en los formularios de pago en el <see cref="TipoPago"/> del
/// dominio y un texto descriptivo. Un valor vacio devuelve nulo: el backend lo rechaza con
/// el mensaje de la regla que corresponda (RN-04 al reservar, HU-06 al cerrar la cuenta).
/// </summary>
public static class MediosPago
{
    /// <summary>Opciones de la pantalla de facturacion de recepcion (HU-06).</summary>
    public static readonly IReadOnlyList<string> DeRecepcion =
        ["Efectivo", "Datafono - Visa **** 4242", "Datafono - Mastercard **** 5555", "Transferencia bancaria"];

    /// <summary>Respaldo de pago de una reserva de habitacion o de tour (RN-04).</summary>
    public static (TipoPago? Tipo, string? Detalle) DeReserva(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return (null, null);

        return valor.Trim().ToLowerInvariant() switch
        {
            "visa" => (TipoPago.Tarjeta, "Visa **** 4242"),
            "mc" => (TipoPago.Tarjeta, "Mastercard **** 5555"),
            "transfer" => (TipoPago.Transferencia, "Transferencia bancaria"),
            "efectivo" => (TipoPago.Deposito, "Deposito en efectivo"),
            "deposito" => (TipoPago.Deposito, "Deposito registrado"),
            _ => (TipoPago.Deposito, valor.Trim()),
        };
    }

    /// <summary>Medio con el que el huesped paga la cuenta en el check-out (HU-06).</summary>
    public static (TipoPago? Tipo, string? Detalle) DeCobro(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor)) return (null, null);

        var texto = valor.Trim();
        var minusculas = texto.ToLowerInvariant();

        if (minusculas.Contains("visa") || minusculas.Contains("mastercard") || minusculas.Contains("datafono"))
            return (TipoPago.Tarjeta, texto);
        if (minusculas.Contains("transferencia"))
            return (TipoPago.Transferencia, texto);

        return (TipoPago.Efectivo, texto);
    }
}
