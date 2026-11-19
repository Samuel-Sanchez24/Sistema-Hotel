using HotelSystem.Application.Abstracciones;
using HotelSystem.Persistence.Repositorios;
using HotelSystem.Persistence.Semilla;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSystem.Persistence;

/// <summary>Registro de la capa de persistencia. Lo llaman HotelSystem.Web y HotelSystem.API.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddPersistencia(this IServiceCollection services, OpcionesBaseDeDatos opciones)
    {
        var activa = ResolverBaseDeDatos(opciones);

        services.AddSingleton(opciones);
        services.AddSingleton(activa);

        services.AddDbContext<HotelDbContext>(builder =>
        {
            // Las consultas que cargan varias colecciones se ejecutan en una sola sentencia.
            if (activa.Proveedor == ProveedorBaseDeDatos.SqlServer)
            {
                builder.UseSqlServer(activa.CadenaConexion, sql => sql.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery));
            }
            else
            {
                builder.UseSqlite(activa.CadenaConexion, sqlite => sqlite.UseQuerySplittingBehavior(QuerySplittingBehavior.SingleQuery));
            }
        });

        services.AddScoped<IUnidadDeTrabajo, UnidadDeTrabajo>();
        services.AddScoped<IPersonaRepository, PersonaRepository>();
        services.AddScoped<IHabitacionRepository, HabitacionRepository>();
        services.AddScoped<IReservaHabitacionRepository, ReservaHabitacionRepository>();
        services.AddScoped<ICuentaRepository, CuentaRepository>();
        services.AddScoped<ITourRepository, TourRepository>();
        services.AddScoped<IServicioAdicionalRepository, ServicioAdicionalRepository>();
        services.AddScoped<IRegistroOperacionRepository, RegistroOperacionRepository>();
        services.AddScoped<SembradorDatos>();

        return services;
    }

    /// <summary>
    /// Crea la base de datos y sus tablas si no existen y carga los datos de demostración
    /// la primera vez. Se llama una vez al arrancar la aplicación.
    /// </summary>
    public static BaseDeDatosActiva InicializarBaseDeDatos(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var proveedor = scope.ServiceProvider;
        var opciones = proveedor.GetRequiredService<OpcionesBaseDeDatos>();
        var db = proveedor.GetRequiredService<HotelDbContext>();

        // La web y la API comparten base de datos. Si arrancan a la vez, este candado del
        // sistema operativo hace que una espere a que la otra termine de crearla.
        using var candado = new Mutex(false, "HotelFastAndFurious.InicializarBaseDeDatos");
        var tomado = false;
        try
        {
            try
            {
                tomado = candado.WaitOne(TimeSpan.FromMinutes(2));
            }
            catch (AbandonedMutexException)
            {
                tomado = true;
            }

            if (opciones.ReiniciarAlIniciar)
            {
                db.Database.EnsureDeleted();
            }

            db.Database.EnsureCreated();
            proveedor.GetRequiredService<SembradorDatos>().Sembrar();
        }
        finally
        {
            if (tomado) candado.ReleaseMutex();
        }

        return proveedor.GetRequiredService<BaseDeDatosActiva>();
    }

    /// <summary>
    /// Borra la base de datos y la vuelve a crear con los datos de demostración.
    /// Pensado para dejar el sistema limpio antes de una sustentación.
    /// </summary>
    public static void ReiniciarBaseDeDatos(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<HotelDbContext>();

        db.Database.EnsureDeleted();
        db.Database.EnsureCreated();
        scope.ServiceProvider.GetRequiredService<SembradorDatos>().Sembrar();
    }

    private static BaseDeDatosActiva ResolverBaseDeDatos(OpcionesBaseDeDatos opciones)
    {
        var pedido = (opciones.Proveedor ?? "Auto").Trim();

        if (pedido.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
        {
            return Sqlite(opciones, "SQLite (configurado en appsettings.json)");
        }

        if (pedido.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
        {
            return new BaseDeDatosActiva(ProveedorBaseDeDatos.SqlServer, opciones.SqlServer, "SQL Server");
        }

        // Auto: SQL Server si responde; si no, SQLite para que el sistema arranque de todas formas.
        if (SqlServerResponde(opciones.SqlServer, out var motivo))
        {
            return new BaseDeDatosActiva(ProveedorBaseDeDatos.SqlServer, opciones.SqlServer, "SQL Server");
        }

        return Sqlite(opciones, $"SQLite, porque SQL Server no respondio ({motivo})");
    }

    private static BaseDeDatosActiva Sqlite(OpcionesBaseDeDatos opciones, string descripcion)
    {
        var cadena = opciones.Sqlite;
        if (string.IsNullOrWhiteSpace(cadena))
        {
            // Carpeta de datos del usuario: así la web y la API usan el mismo archivo.
            var carpeta = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HotelFastAndFurious");
            Directory.CreateDirectory(carpeta);
            cadena = $"Data Source={Path.Combine(carpeta, "hotel.db")}";
        }

        return new BaseDeDatosActiva(ProveedorBaseDeDatos.Sqlite, cadena, descripcion);
    }

    private static bool SqlServerResponde(string cadenaConexion, out string motivo)
    {
        motivo = string.Empty;
        try
        {
            // Se prueba contra "master" porque la base del hotel puede no existir todavía.
            var constructor = new SqlConnectionStringBuilder(cadenaConexion)
            {
                InitialCatalog = "master",
                ConnectTimeout = 15,
            };

            using var conexion = new SqlConnection(constructor.ConnectionString);
            conexion.Open();
            return true;
        }
        catch (Exception ex)
        {
            motivo = ex.Message.Split('\n')[0].Trim();
            return false;
        }
    }
}
