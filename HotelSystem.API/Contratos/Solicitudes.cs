namespace HotelSystem.API.Contratos;

// Cuerpos JSON que recibe la API. Los nombres siguen el contrato del README del front
// (camelCase en el JSON). Los enums llegan como texto y se leen con LectorValores.

public sealed record LoginSolicitud(string? Usuario, string? Contrasena);

public sealed record RegistroHuespedSolicitud(
    string? Nombre,
    string? Correo,
    DateOnly? FechaNacimiento,
    string? Telefono,
    string? Usuario,
    string? Contrasena);

public sealed record CambioEstadoHabitacionSolicitud(string? Estado, string? Nota);

/// <param name="NumeroHabitacion">Numero de la habitacion elegida (por ejemplo "305").</param>
/// <param name="MayorDeEdad">Declaracion del formulario (RN-10); ademas se valida la fecha de nacimiento.</param>
public sealed record ReservaHabitacionSolicitud(
    string? NumeroHabitacion,
    DateOnly? FechaIngreso,
    DateOnly? FechaSalida,
    int CantidadHuespedes,
    string? MetodoPago,
    string? DetallePago,
    bool MayorDeEdad);

public sealed record CierreCuentaSolicitud(decimal MontoPagado, string? MetodoPago, string? DetallePago);

public sealed record ReservaTourSolicitud(int NumeroPersonas, string? MetodoPago, string? DetallePago, bool MayorDeEdad);

public sealed record CuposSolicitud(int Cantidad);

public sealed record AsignacionSolicitud(int IdGuia, int IdVehiculo);

public sealed record NuevaSolicitudServicio(int IdServicio);

public sealed record CambioEstadoSolicitudServicio(string? Estado);

/// <param name="Cantidad">Por defecto 1.</param>
/// <param name="Fecha">Fecha del consumo. Si se omite es hoy; otra fecha se rechaza (RN-06).</param>
public sealed record ConsumoSolicitud(
    string? NumeroHabitacion,
    int? IdServicio,
    string? NombreServicio,
    int? Cantidad,
    decimal PrecioAplicado,
    DateOnly? Fecha);

public sealed record NuevoUsuarioSolicitud(string? Nombre, string? Correo, string? Rol);

public sealed record CambioRolSolicitud(string? Rol);

public sealed record CambioEstadoUsuarioSolicitud(bool Activo);
