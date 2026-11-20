using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Domain.Interfaces;

/// <summary>
/// Algo que se puede pagar. Solo la implementa <c>Cuenta</c>: los tours no se pagan por
/// separado, se cargan a la cuenta única de la estancia (RN-07). Por eso está separada de
/// <see cref="ICalculable"/> (principio de segregación de interfaces): <c>ReservaTour</c>
/// calcula su total pero no se le obliga a implementar un pago que no le corresponde.
/// </summary>
public interface IPagable
{
    bool EstaPagada { get; }

    void RegistrarPago(decimal monto, TipoPago medio, string? detalle, DateTime fecha);
}
