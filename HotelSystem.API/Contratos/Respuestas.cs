using HotelSystem.Application.CierreCuenta;
using HotelSystem.Application.Housekeeping;
using HotelSystem.Application.Reservas;
using HotelSystem.Application.Usuarios;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.API.Contratos;

// Respuestas que no son directamente un DTO de la capa de aplicacion: se adaptan a la
// forma que pide el contrato del front. El resto de rutas devuelve los DTOs tal cual.

/// <summary>Cuerpo de todas las respuestas de error (4xx y 500).</summary>
public sealed record RespuestaError(string Mensaje, string? Regla = null);

public sealed record UsuarioSesion(int Id, string Nombre, string Correo, string Usuario, string Rol, string? NumeroHabitacion);

public sealed record LoginRespuesta(string Token, DateTime Expira, UsuarioSesion Usuario);

/// <summary>Llegada o salida del dia (HU-06).</summary>
/// <param name="Realizado">El check-in (o el check-out) ya se registro.</param>
public sealed record MovimientoRespuesta(
    int IdReserva,
    string Codigo,
    string Huesped,
    int IdHabitacion,
    string NumeroHabitacion,
    EstadoHabitacion EstadoHabitacion,
    DateOnly FechaIngreso,
    DateOnly FechaSalida,
    int Noches,
    EstadoReserva Estado,
    bool Realizado)
{
    public static MovimientoRespuesta Desde(MovimientoDto m, bool realizado) => new(
        m.ReservaId, m.Codigo, m.Huesped, m.HabitacionId, m.NumeroHabitacion, m.EstadoHabitacion,
        m.FechaEntrada, m.FechaSalida, m.Noches, m.Estado, realizado);
}

/// <summary>Resultado de cancelar una reserva (RN-09: cargo de una noche si es tardia).</summary>
public sealed record CancelacionRespuesta(string Id, bool Tardia, decimal Cargo);

/// <summary>Prefactura final del check-out (HU-06) con el detalle completo de la cuenta.</summary>
public sealed record CierreCuentaRespuesta(
    int? IdPrefactura,
    DateTime? FechaGeneracion,
    decimal MontoTotal,
    string NumeroHabitacion,
    string Huesped,
    string? MetodoPago,
    PrefacturaDto Detalle)
{
    public static CierreCuentaRespuesta Desde(PrefacturaDto p) => new(
        p.IdPrefactura, p.FechaGeneracion, p.MontoConsolidado, p.NumeroHabitacion, p.NombreHuesped, p.MetodoPago, p);
}

/// <summary>Habitacion del plan de limpieza (HU-11, RF-10). Las que reciben huesped hoy van primero.</summary>
public sealed record TareaLimpiezaRespuesta(
    int Id,
    string Numero,
    TipoHabitacion Tipo,
    EstadoHabitacion Estado,
    int Piso,
    DateTime UltimaActualizacion,
    string? NotaMantenimiento,
    bool LlegadaHoy)
{
    public static TareaLimpiezaRespuesta Desde(TareaLimpiezaDto t) => new(
        t.Habitacion.Id, t.Habitacion.Numero, t.Habitacion.Tipo, t.Habitacion.Estado, t.Habitacion.Piso,
        t.Habitacion.UltimaActualizacion, t.Habitacion.NotaMantenimiento, t.LlegadaHoy);
}

/// <summary>Empleado recien creado (HU-01) con su contrasena temporal, que solo se entrega esta vez.</summary>
public sealed record UsuarioCreadoRespuesta(int Id, string Nombre, string Correo, string Usuario, RolEmpleado Rol, bool Activo, string ContrasenaTemporal)
{
    public static UsuarioCreadoRespuesta Desde(EmpleadoCreadoDto creado) => new(
        creado.Empleado.Id, creado.Empleado.Nombre, creado.Empleado.Correo, creado.Empleado.Usuario,
        creado.Empleado.Rol, creado.Empleado.Activo, creado.ContrasenaTemporal);
}
