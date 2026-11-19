using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Estados;
using HotelSystem.Domain.Excepciones;
using HotelSystem.Domain.Interfaces;

namespace HotelSystem.Domain.Entidades;

/// <summary>
/// Habitación del hotel. Participa en dos patrones:
///
/// - State: es el contexto. Cada operación de cambio de estado se delega en el objeto
///   <see cref="IEstadoHabitacion"/> que corresponde a su estado actual, y ese objeto decide
///   si la transición es válida (RN-02).
/// - Observer: es el sujeto. Cada cambio de estado se notifica a los observadores suscritos
///   (los paneles de recepción y de housekeeping).
/// </summary>
public class Habitacion
{
    private readonly List<IObservadorHabitacion> _observadores = new();

    private Habitacion()
    {
        Numero = string.Empty;
    }

    public Habitacion(string numero, TipoHabitacion tipo, int piso, int capacidad, decimal precioNoche, DateTime ahora)
    {
        if (string.IsNullOrWhiteSpace(numero)) throw new ReglaNegocioException("El numero de habitacion es obligatorio.");
        if (capacidad < 1) throw new ReglaNegocioException("La capacidad debe ser de al menos una persona.");
        if (precioNoche <= 0) throw new ReglaNegocioException("La tarifa por noche debe ser mayor que cero.");

        Numero = numero.Trim();
        Tipo = tipo;
        Piso = piso;
        Capacidad = capacidad;
        PrecioNoche = precioNoche;
        Estado = EstadoHabitacion.Disponible;
        UltimaActualizacion = ahora;
    }

    public int Id { get; private set; }

    public string Numero { get; private set; }

    public TipoHabitacion Tipo { get; private set; }

    public int Piso { get; private set; }

    /// <summary>Número máximo de personas (RN-11).</summary>
    public int Capacidad { get; private set; }

    public decimal PrecioNoche { get; private set; }

    /// <summary>Estado persistido. Solo cambia a través de las operaciones de esta clase.</summary>
    public EstadoHabitacion Estado { get; private set; }

    /// <summary>Descripción de la incidencia mientras la habitación está en mantenimiento.</summary>
    public string? NotaMantenimiento { get; private set; }

    public DateTime UltimaActualizacion { get; private set; }

    /// <summary>Objeto de estado (patrón State) que corresponde a <see cref="Estado"/>.</summary>
    public IEstadoHabitacion EstadoActual => EstadosHabitacion.De(Estado);

    /* ─── Operaciones (patrón State): la decisión la toma el estado actual ─── */

    public void Ocupar(DateTime ahora) => EstadoActual.Ocupar(this, ahora);

    public void Liberar(DateTime ahora) => EstadoActual.Liberar(this, ahora);

    public void MarcarDisponible(DateTime ahora) => EstadoActual.MarcarDisponible(this, ahora);

    public void EnviarALimpieza(DateTime ahora) => EstadoActual.EnviarALimpieza(this, ahora);

    public void EnviarAMantenimiento(string? nota, DateTime ahora)
    {
        if (string.IsNullOrWhiteSpace(nota))
            throw new ReglaNegocioException("Debe ingresar una descripcion de la incidencia (HU-11 Esc.3).");
        EstadoActual.EnviarAMantenimiento(this, nota.Trim(), ahora);
    }

    /// <summary>RN-11: la cantidad de huéspedes no puede superar la capacidad.</summary>
    public bool AdmiteHuespedes(int cantidad) => cantidad >= 1 && cantidad <= Capacidad;

    /* ─── Patrón Observer: sujeto ─── */

    public void Suscribir(IObservadorHabitacion observador)
    {
        if (!_observadores.Contains(observador)) _observadores.Add(observador);
    }

    public void Desuscribir(IObservadorHabitacion observador) => _observadores.Remove(observador);

    /// <summary>
    /// Aplica el cambio y avisa a los observadores. Es interno a propósito: solo lo llaman
    /// los estados concretos, de modo que nadie fuera del dominio puede saltarse la RN-02.
    /// </summary>
    internal void CambiarEstado(EstadoHabitacion nuevo, DateTime ahora, string? nota)
    {
        var anterior = Estado;
        Estado = nuevo;
        UltimaActualizacion = ahora;
        NotaMantenimiento = nuevo == EstadoHabitacion.Mantenimiento ? nota : null;
        Notificar(anterior);
    }

    private void Notificar(EstadoHabitacion anterior)
    {
        foreach (var observador in _observadores.ToArray())
            observador.Actualizar(this, anterior);
    }
}
