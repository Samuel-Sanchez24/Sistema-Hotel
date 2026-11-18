namespace HotelSystem.Domain.Entidades;

/// <summary>Huésped del hotel: reserva habitaciones y tours y solicita servicios.</summary>
public class Huesped : Persona
{
    public const string NombreRol = "Huesped";

    private Huesped()
    {
    }

    public Huesped(string nombre, string correo, string usuario, string claveHash, DateOnly? fechaNacimiento, DateTime fechaRegistro, string? telefono = null)
        : base(nombre, correo, usuario, claveHash, fechaNacimiento, fechaRegistro)
    {
        Telefono = string.IsNullOrWhiteSpace(telefono) ? null : telefono.Trim();
    }

    public string? Telefono { get; private set; }

    public override string RolNombre => NombreRol;
}
