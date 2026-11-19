using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Domain.Estados;

/// <summary>
/// Devuelve el objeto de estado que corresponde al valor guardado en la base de datos.
/// Los estados no tienen datos propios, así que se comparte una sola instancia de cada uno.
/// </summary>
public static class EstadosHabitacion
{
    private static readonly IEstadoHabitacion Disponible = new EstadoDisponible();
    private static readonly IEstadoHabitacion Ocupada = new EstadoOcupada();
    private static readonly IEstadoHabitacion EnLimpieza = new EstadoEnLimpieza();
    private static readonly IEstadoHabitacion Mantenimiento = new EstadoMantenimiento();

    public static IEstadoHabitacion De(EstadoHabitacion codigo) => codigo switch
    {
        EstadoHabitacion.Disponible => Disponible,
        EstadoHabitacion.Ocupada => Ocupada,
        EstadoHabitacion.EnLimpieza => EnLimpieza,
        EstadoHabitacion.Mantenimiento => Mantenimiento,
        _ => throw new ArgumentOutOfRangeException(nameof(codigo), codigo, "Estado de habitacion desconocido."),
    };
}
