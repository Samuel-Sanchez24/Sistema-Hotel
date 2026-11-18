namespace HotelSystem.Domain.Excepciones;

/// <summary>
/// Se lanza cuando se pide un registro que no existe (habitación, reserva, tour, usuario...).
/// Hereda de <see cref="ReglaNegocioException"/> para que la web la trate igual; la API la
/// convierte en un 404.
/// </summary>
public class EntidadNoEncontradaException : ReglaNegocioException
{
    public EntidadNoEncontradaException(string mensaje) : base(mensaje)
    {
    }
}
