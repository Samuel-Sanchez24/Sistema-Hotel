namespace HotelSystem.Application.Abstracciones;

/// <summary>
/// Confirma en la base de datos los cambios hechos a las entidades durante una operación.
/// </summary>
public interface IUnidadDeTrabajo
{
    void GuardarCambios();

    /// <summary>
    /// Ejecuta la acción dentro de una transacción serializable: si dos peticiones intentan
    /// reservar la misma habitación o el mismo cupo a la vez, solo una lo consigue
    /// (RN-01, RN-05, RNF-07). Si ya hay una transacción abierta, se une a ella.
    /// </summary>
    void EnTransaccion(Action accion);

    T EnTransaccion<T>(Func<T> accion);
}
