namespace HotelSystem.Domain.Interfaces;

/// <summary>
/// Algo que tiene un valor total en dinero. La implementan <c>Cuenta</c> y <c>ReservaTour</c>.
///
/// Contrato (lo que debe cumplir cualquier implementación, principio de sustitución de Liskov):
/// el total nunca es negativo y llamar al método no cambia el estado del objeto, así que dos
/// llamadas seguidas devuelven lo mismo. Las pruebas de contrato están en HotelSystem.Tests.
/// </summary>
public interface ICalculable
{
    decimal CalcularTotal();
}
