namespace HotelSystem.Domain.Excepciones;

/// <summary>
/// Se lanza cuando una operación incumple una regla de negocio (RN-01 a RN-11) o una
/// validación del dominio. El mensaje está escrito para mostrarse tal cual al usuario.
/// </summary>
public class ReglaNegocioException : Exception
{
    public ReglaNegocioException(string mensaje, string? regla = null) : base(mensaje)
    {
        Regla = regla;
    }

    /// <summary>Identificador de la regla incumplida (por ejemplo "RN-01"), si aplica.</summary>
    public string? Regla { get; }
}
