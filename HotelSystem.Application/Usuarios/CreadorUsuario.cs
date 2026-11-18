using System.Security.Cryptography;
using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.Comun;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Application.Usuarios;

/// <summary>
/// Patrón Factory Method (creador abstracto).
///
/// <see cref="Registrar"/> define el flujo común de alta de un usuario: validar los datos,
/// generar las credenciales, crear el objeto, guardarlo y dejar constancia en la bitácora.
/// El único paso que cambia es QUÉ objeto se crea, y esa decisión se deja a las subclases
/// mediante el método fábrica <see cref="CrearUsuario"/>:
/// <see cref="CreadorHuesped"/> crea un <see cref="Huesped"/> y
/// <see cref="CreadorEmpleado"/> crea un <see cref="Empleado"/>.
///
/// Los productos son solo dos (Huesped y Empleado). El rol del empleado es un atributo,
/// no una subclase, así que no hay un creador por cada rol.
/// </summary>
public abstract class CreadorUsuario
{
    private readonly IPersonaRepository _personas;
    private readonly IHasherContrasenas _hasher;
    private readonly Bitacora _bitacora;

    protected CreadorUsuario(IPersonaRepository personas, IHasherContrasenas hasher, IReloj reloj, Bitacora bitacora)
    {
        _personas = personas;
        _hasher = hasher;
        _bitacora = bitacora;
        Reloj = reloj;
    }

    protected IReloj Reloj { get; }

    /// <summary>
    /// Flujo común de registro. No confirma en la base de datos: eso lo hace el servicio
    /// que lo invoca con IUnidadDeTrabajo.GuardarCambios().
    /// </summary>
    public UsuarioCreado Registrar(DatosNuevoUsuario datos)
    {
        Validar(datos);

        var usuario = ResolverNombreUsuario(datos);
        var esTemporal = string.IsNullOrEmpty(datos.Contrasena);
        var contrasena = esTemporal ? GenerarContrasenaTemporal() : datos.Contrasena!;

        var persona = CrearUsuario(datos, usuario, _hasher.Hash(contrasena));

        _personas.Agregar(persona);
        _bitacora.Registrar(datos.RegistradoPorId, TipoOperacion.GestionUsuarios, $"Usuario creado: {persona.Nombre} ({persona.RolNombre})");

        return new UsuarioCreado(persona, contrasena, esTemporal);
    }

    /// <summary>Método fábrica: cada subclase decide qué tipo de <see cref="Persona"/> crea.</summary>
    protected abstract Persona CrearUsuario(DatosNuevoUsuario datos, string usuario, string claveHash);

    /// <summary>Validaciones propias de cada tipo de usuario. Por defecto no hay ninguna.</summary>
    protected virtual void ValidarDatosPropios(DatosNuevoUsuario datos)
    {
    }

    private void Validar(DatosNuevoUsuario datos)
    {
        if (string.IsNullOrWhiteSpace(datos.Nombre) || string.IsNullOrWhiteSpace(datos.Correo))
            throw new ReglaNegocioException("Complete nombre y correo.");

        var correo = datos.Correo.Trim().ToLowerInvariant();
        if (!correo.Contains('@'))
            throw new ReglaNegocioException("El correo no es valido.");
        if (_personas.ExisteCorreo(correo))
            throw new ReglaNegocioException($"Ya existe un usuario con el correo {correo}.");
        if (!string.IsNullOrEmpty(datos.Contrasena) && datos.Contrasena.Length < 6)
            throw new ReglaNegocioException("La contrasena debe tener al menos 6 caracteres.");

        ValidarDatosPropios(datos);
    }

    /// <summary>Usa el usuario pedido o lo deriva del correo (mlopez@hotel.com -> mlopez), evitando repetidos.</summary>
    private string ResolverNombreUsuario(DatosNuevoUsuario datos)
    {
        if (!string.IsNullOrWhiteSpace(datos.Usuario))
        {
            var pedido = datos.Usuario.Trim().ToLowerInvariant();
            if (_personas.ExisteUsuario(pedido))
                throw new ReglaNegocioException($"El usuario {pedido} ya existe.");
            return pedido;
        }

        var raiz = datos.Correo.Trim().ToLowerInvariant().Split('@')[0];
        var candidato = raiz;
        var sufijo = 2;
        while (_personas.ExisteUsuario(candidato))
        {
            candidato = raiz + sufijo;
            sufijo++;
        }

        return candidato;
    }

    private static string GenerarContrasenaTemporal() =>
        "Hotel-" + RandomNumberGenerator.GetInt32(100000, 1000000);
}
