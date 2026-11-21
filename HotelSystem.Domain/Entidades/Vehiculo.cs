using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Domain.Entidades;

/// <summary>Vehículo (van, bus o lancha) asignable a un tour. Tampoco puede cruzarse de horario (RN-03).</summary>
public class Vehiculo
{
    private Vehiculo()
    {
        Nombre = string.Empty;
        Placa = string.Empty;
        Modelo = string.Empty;
    }

    public Vehiculo(string nombre, string placa, string modelo, int capacidadPasajeros)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ReglaNegocioException("El nombre del vehiculo es obligatorio.");
        if (capacidadPasajeros < 1) throw new ReglaNegocioException("La capacidad del vehiculo debe ser de al menos un pasajero.");

        Nombre = nombre.Trim();
        Placa = placa.Trim();
        Modelo = modelo.Trim();
        CapacidadPasajeros = capacidadPasajeros;
        Activo = true;
    }

    public int Id { get; private set; }

    /// <summary>Nombre con el que lo conoce el coordinador, por ejemplo "Van #001".</summary>
    public string Nombre { get; private set; }

    public string Placa { get; private set; }

    public string Modelo { get; private set; }

    public int CapacidadPasajeros { get; private set; }

    public bool Activo { get; private set; }

    /// <summary>El vehículo sirve para el grupo si caben todas las personas.</summary>
    public bool EsAptoParaGrupo(int personas) => personas <= CapacidadPasajeros;
}
