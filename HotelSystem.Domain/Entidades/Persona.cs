using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Domain.Entidades;

/// <summary>
/// Usuario del sistema. Clase base de <see cref="Huesped"/> y <see cref="Empleado"/>.
/// La contraseña nunca se guarda en claro: solo su hash.
/// </summary>
public abstract class Persona
{
    /// <summary>Constructor que usa Entity Framework al leer de la base de datos.</summary>
    protected Persona()
    {
        Nombre = string.Empty;
        Correo = string.Empty;
        Usuario = string.Empty;
        ClaveHash = string.Empty;
    }

    protected Persona(string nombre, string correo, string usuario, string claveHash, DateOnly? fechaNacimiento, DateTime fechaRegistro)
    {
        if (string.IsNullOrWhiteSpace(nombre)) throw new ReglaNegocioException("El nombre es obligatorio.");
        if (string.IsNullOrWhiteSpace(correo) || !correo.Contains('@')) throw new ReglaNegocioException("El correo no es valido.");
        if (string.IsNullOrWhiteSpace(usuario)) throw new ReglaNegocioException("El nombre de usuario es obligatorio.");
        if (string.IsNullOrWhiteSpace(claveHash)) throw new ReglaNegocioException("La contrasena es obligatoria.");

        Nombre = nombre.Trim();
        Correo = correo.Trim().ToLowerInvariant();
        Usuario = usuario.Trim().ToLowerInvariant();
        ClaveHash = claveHash;
        FechaNacimiento = fechaNacimiento;
        FechaRegistro = fechaRegistro;
        Activo = true;
    }

    public int Id { get; private set; }

    public string Nombre { get; private set; }

    public string Correo { get; private set; }

    /// <summary>Nombre de usuario para iniciar sesión. Se guarda en minúsculas.</summary>
    public string Usuario { get; private set; }

    public string ClaveHash { get; private set; }

    public DateOnly? FechaNacimiento { get; private set; }

    /// <summary>Una cuenta desactivada no puede iniciar sesión (HU-01 Esc.3, HU-02 Esc.3).</summary>
    public bool Activo { get; private set; }

    public DateTime FechaRegistro { get; private set; }

    /// <summary>Nombre del rol con el que entra al sistema: "Huesped" o el rol del empleado.</summary>
    public abstract string RolNombre { get; }

    /// <summary>RN-10. Si no se conoce la fecha de nacimiento no se puede dar por cumplida.</summary>
    public bool EsMayorDeEdad(DateOnly hoy) =>
        FechaNacimiento is { } nacimiento && nacimiento.AddYears(18) <= hoy;

    public void Desactivar() => Activo = false;

    public void Reactivar() => Activo = true;

    public void CambiarClave(string nuevoHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoHash)) throw new ReglaNegocioException("La contrasena es obligatoria.");
        ClaveHash = nuevoHash;
    }
}
