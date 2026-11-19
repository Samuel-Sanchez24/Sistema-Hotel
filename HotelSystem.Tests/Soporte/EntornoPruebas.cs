using HotelSystem.Application;
using HotelSystem.Application.Abstracciones;
using HotelSystem.Infrastructure;
using HotelSystem.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSystem.Tests.Soporte;

/// <summary>Reloj con una fecha y hora fijas: las pruebas no dependen del dia en que se ejecutan.</summary>
public sealed class RelojFijo(DateTime ahora) : IReloj
{
    public DateTime Ahora { get; set; } = ahora;

    public DateOnly Hoy => DateOnly.FromDateTime(Ahora);
}

/// <summary>Hash trivial para que la siembra de 24 usuarios sea instantanea (PBKDF2 es lento a proposito).</summary>
public sealed class HasherFalso : IHasherContrasenas
{
    public string Hash(string contrasena) => "prueba$" + contrasena;

    public bool Verificar(string contrasena, string hash) => hash == "prueba$" + contrasena;
}

/// <summary>Correo que no sale a ningun lado: guarda los mensajes para revisarlos en la prueba (HU-04).</summary>
public sealed class CorreoFalso : IServicioCorreo
{
    private readonly List<MensajeCorreo> _enviados = [];

    public IReadOnlyList<MensajeCorreo> Enviados
    {
        get
        {
            lock (_enviados) return _enviados.ToList();
        }
    }

    public bool Enviar(MensajeCorreo mensaje)
    {
        lock (_enviados) _enviados.Add(mensaje);
        return true;
    }
}

/// <summary>
/// Sistema completo para una prueba: las mismas capas y la misma inyeccion de dependencias
/// que usan la web y la API, sobre una base de datos SQLite nueva en un archivo temporal,
/// sembrada con los datos de demostracion. Solo se reemplazan el reloj, el hash y el correo.
/// </summary>
public sealed class EntornoPruebas : IDisposable
{
    /// <summary>"Hoy" de todas las pruebas: miercoles 9 de septiembre de 2026, 12:00 m.</summary>
    public static readonly DateTime Ahora = new(2026, 9, 9, 12, 0, 0);

    private readonly string _archivo;
    private readonly ServiceProvider _proveedor;

    public EntornoPruebas()
    {
        _archivo = Path.Combine(Path.GetTempPath(), $"hotel-pruebas-{Guid.NewGuid():N}.db");

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddAplicacion();
        services.AddPersistencia(new OpcionesBaseDeDatos { Proveedor = "Sqlite", Sqlite = $"Data Source={_archivo}" });
        services.AddInfraestructura();

        // Reemplazos: el ultimo registro de una interfaz es el que se resuelve.
        services.AddSingleton<IReloj>(Reloj);
        services.AddSingleton<IHasherContrasenas>(new HasherFalso());
        services.AddSingleton<IServicioCorreo>(Correo);

        // ValidateOnBuild comprueba que todas las dependencias registradas se pueden construir.
        _proveedor = services.BuildServiceProvider(new ServiceProviderOptions { ValidateOnBuild = true, ValidateScopes = true });
        _proveedor.InicializarBaseDeDatos();
    }

    public RelojFijo Reloj { get; } = new(Ahora);

    public CorreoFalso Correo { get; } = new();

    public DateOnly Hoy => Reloj.Hoy;

    /// <summary>Servicio sin estado de peticion (paneles del Observer, exportadores...).</summary>
    public T Unico<T>() where T : notnull => _proveedor.GetRequiredService<T>();

    /// <summary>Ejecuta una operacion en un ambito nuevo, como una peticion web independiente.</summary>
    public TResultado En<TServicio, TResultado>(Func<TServicio, TResultado> operacion) where TServicio : notnull
    {
        using var ambito = _proveedor.CreateScope();
        return operacion(ambito.ServiceProvider.GetRequiredService<TServicio>());
    }

    public void En<TServicio>(Action<TServicio> operacion) where TServicio : notnull
    {
        using var ambito = _proveedor.CreateScope();
        operacion(ambito.ServiceProvider.GetRequiredService<TServicio>());
    }

    public int IdPersona(string usuario) =>
        En<IPersonaRepository, int>(r => r.BuscarPorUsuario(usuario)?.Id ?? throw new InvalidOperationException($"No existe {usuario}"));

    public int IdHabitacion(string numero) =>
        En<IHabitacionRepository, int>(r => r.ObtenerPorNumero(numero)?.Id ?? throw new InvalidOperationException($"No existe {numero}"));

    public void Dispose()
    {
        _proveedor.Dispose();
        SqliteConnection.ClearAllPools();
        try
        {
            File.Delete(_archivo);
        }
        catch (IOException)
        {
            // Si el archivo sigue en uso se deja en la carpeta temporal.
        }
    }
}
