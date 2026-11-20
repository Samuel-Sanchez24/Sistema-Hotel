namespace HotelSystem.Application.CierreCuenta;

/// <summary>Cuenta consolidada y cierre en el check-out (HU-06, HU-12). La implementa <see cref="CuentaFacade"/>.</summary>
public interface ICuentaService
{
    /// <summary>Cuenta de la estancia en curso en la habitación. Nulo si no hay estancia con cuenta abierta.</summary>
    PrefacturaDto? ConsultarPorHabitacion(int habitacionId);

    PrefacturaDto? ConsultarPorNumero(string numeroHabitacion);

    /// <summary>Cuenta de la estancia en curso del huésped ("Mi cuenta"). Nulo si no tiene check-in activo.</summary>
    PrefacturaDto? ConsultarDeHuesped(int huespedId);

    /// <summary>Genera y guarda la prefactura de la estancia en curso.</summary>
    PrefacturaDto GenerarPrefactura(int habitacionId, int empleadoId);

    /// <summary>
    /// Check-out completo en una sola operación: consolida la cuenta, valida el pago, cierra la
    /// cuenta (RN-07), registra la salida (habitación a limpieza, RN-02) y genera la prefactura final.
    /// </summary>
    PrefacturaDto CerrarCuenta(SolicitudCierreCuenta solicitud);
}
