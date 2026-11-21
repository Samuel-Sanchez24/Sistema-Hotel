using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Domain.Entidades;

/// <summary>Guía turístico que acompaña un tour. No puede estar en dos tours que se crucen (RN-03).</summary>
public class Guia
{
    private Guia()
    {
        Nombre = string.Empty;
    }

    public Guia(string nombre)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ReglaNegocioException("El nombre del guia es obligatorio.");
        Nombre = nombre.Trim();
        Activo = true;
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; }

    public bool Activo { get; private set; }
}
