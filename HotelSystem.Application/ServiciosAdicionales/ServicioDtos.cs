using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;

namespace HotelSystem.Application.ServiciosAdicionales;

/// <summary>Servicio del catálogo. Un precio en cero significa precio variable.</summary>
public sealed record ServicioAdicionalDto(int Id, string Nombre, string Categoria, string Descripcion, decimal PrecioUnitario, bool Activo)
{
    public bool TienePrecioFijo => PrecioUnitario > 0;

    public static ServicioAdicionalDto Desde(ServicioAdicional servicio) =>
        new(servicio.Id, servicio.Nombre, servicio.Categoria, servicio.Descripcion, servicio.PrecioUnitario, servicio.Activo);
}

/// <summary>Solicitud de servicio hecha por un huésped (HU-07).</summary>
public sealed record SolicitudServicioDto(
    int Id,
    int ServicioId,
    string NombreServicio,
    string NumeroHabitacion,
    string NombreHuesped,
    EstadoSolicitud Estado,
    DateTime FechaSolicitud)
{
    public static SolicitudServicioDto Desde(SolicitudServicio solicitud) => new(
        solicitud.Id,
        solicitud.ServicioAdicionalId,
        solicitud.Servicio.Nombre,
        solicitud.Cuenta.Reserva.Habitacion.Numero,
        solicitud.Cuenta.Reserva.Huesped.Nombre,
        solicitud.Estado,
        solicitud.FechaSolicitud);
}

/// <summary>Consumo cargado a la cuenta de una habitación (HU-08).</summary>
public sealed record ConsumoDto(
    int Id,
    string NumeroHabitacion,
    string NombreHuesped,
    int ServicioId,
    string NombreServicio,
    int Cantidad,
    decimal PrecioAplicado,
    decimal Subtotal,
    DateTime FechaHora)
{
    public static ConsumoDto Desde(ConsumoServicio consumo) => new(
        consumo.Id,
        consumo.Cuenta.Reserva.Habitacion.Numero,
        consumo.Cuenta.Reserva.Huesped.Nombre,
        consumo.ServicioAdicionalId,
        consumo.Servicio.Nombre,
        consumo.Cantidad,
        consumo.PrecioAplicado,
        consumo.Subtotal,
        consumo.FechaHora);
}

/// <summary>Datos para registrar un consumo en el punto de servicio.</summary>
/// <param name="ServicioId">Servicio del catálogo. Si no se indica, se busca por <paramref name="NombreServicio"/>.</param>
/// <param name="Fecha">Fecha del consumo. Si no se indica es hoy; cualquier otra se rechaza (RN-06).</param>
public sealed record RegistroConsumo(
    string NumeroHabitacion,
    int? ServicioId,
    string? NombreServicio,
    int Cantidad,
    decimal PrecioAplicado,
    DateOnly? Fecha,
    int EmpleadoId);
