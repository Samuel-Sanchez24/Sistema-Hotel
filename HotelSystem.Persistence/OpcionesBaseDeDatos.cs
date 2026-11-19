namespace HotelSystem.Persistence;

/// <summary>Configuración de la base de datos. Se lee de la sección "BaseDeDatos" de appsettings.json.</summary>
public sealed class OpcionesBaseDeDatos
{
    public const string CadenaLocalDb =
        "Server=(localdb)\\MSSQLLocalDB;Database=HotelFastAndFurious;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

    /// <summary>
    /// "SqlServer", "Sqlite" o "Auto". En Auto se usa SQL Server y, si no responde,
    /// se pasa a un archivo SQLite para que el sistema arranque de todas formas.
    /// </summary>
    public string Proveedor { get; set; } = "Auto";

    /// <summary>Cadena de conexión de SQL Server. Por defecto, el LocalDB que instala Visual Studio.</summary>
    public string SqlServer { get; set; } = CadenaLocalDb;

    /// <summary>Cadena de conexión de SQLite. Vacía = archivo hotel.db en la carpeta de datos del usuario.</summary>
    public string Sqlite { get; set; } = string.Empty;

    /// <summary>Si es verdadero, borra la base de datos al iniciar y la vuelve a crear con los datos de demostración.</summary>
    public bool ReiniciarAlIniciar { get; set; }
}

public enum ProveedorBaseDeDatos
{
    SqlServer,
    Sqlite,
}

/// <summary>Base de datos que quedó en uso al arrancar, para mostrarla en el registro de inicio.</summary>
public sealed record BaseDeDatosActiva(ProveedorBaseDeDatos Proveedor, string CadenaConexion, string Descripcion);
