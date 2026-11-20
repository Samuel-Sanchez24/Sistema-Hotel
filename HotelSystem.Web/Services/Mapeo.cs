using HotelSystem.Application.Comun;
using HotelSystem.Application.Housekeeping;
using HotelSystem.Application.Reservas;
using HotelSystem.Application.ServiciosAdicionales;
using HotelSystem.Application.Tours;
using HotelSystem.Application.Usuarios;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.Helpers;

namespace HotelSystem.Web.Services;

/// <summary>
/// Traduce los resultados de la capa de aplicacion (nombres del diagrama de clases, fechas
/// con tipo) a los DTOs que ya usaban las vistas (nombres del prototipo, fechas como texto).
/// Gracias a esto las vistas Razor no cambiaron al conectar el backend real.
/// </summary>
public static class Mapeo
{
    /* ─── Enumeraciones ─── */

    public static RoomType AWeb(this TipoHabitacion tipo) => tipo switch
    {
        TipoHabitacion.Simple => RoomType.Simple,
        TipoHabitacion.Doble => RoomType.Doble,
        _ => RoomType.Suite,
    };

    public static TipoHabitacion ADominio(this RoomType tipo) => tipo switch
    {
        RoomType.Simple => TipoHabitacion.Simple,
        RoomType.Doble => TipoHabitacion.Doble,
        _ => TipoHabitacion.Suite,
    };

    public static RoomStatus AWeb(this EstadoHabitacion estado) => estado switch
    {
        EstadoHabitacion.Disponible => RoomStatus.Disponible,
        EstadoHabitacion.Ocupada => RoomStatus.Ocupada,
        EstadoHabitacion.EnLimpieza => RoomStatus.EnLimpieza,
        _ => RoomStatus.Mantenimiento,
    };

    public static EstadoHabitacion ADominio(this RoomStatus estado) => estado switch
    {
        RoomStatus.Disponible => EstadoHabitacion.Disponible,
        RoomStatus.Ocupada => EstadoHabitacion.Ocupada,
        RoomStatus.EnLimpieza => EstadoHabitacion.EnLimpieza,
        _ => EstadoHabitacion.Mantenimiento,
    };

    public static Role AWeb(this RolEmpleado rol) => rol switch
    {
        RolEmpleado.Recepcion => Role.Recepcion,
        RolEmpleado.Housekeeping => Role.Housekeeping,
        RolEmpleado.CoordinadorTours => Role.Coordinador,
        RolEmpleado.Servicios => Role.Servicios,
        _ => Role.Admin,
    };

    /// <summary>Rol del personal equivalente. Nulo para <see cref="Role.Huesped"/>, que no es un empleado.</summary>
    public static RolEmpleado? ADominio(this Role rol) => rol switch
    {
        Role.Recepcion => RolEmpleado.Recepcion,
        Role.Housekeeping => RolEmpleado.Housekeeping,
        Role.Coordinador => RolEmpleado.CoordinadorTours,
        Role.Servicios => RolEmpleado.Servicios,
        Role.Admin => RolEmpleado.Administrador,
        _ => null,
    };

    /// <summary>Perfil de la pantalla de login que le corresponde al usuario autenticado.</summary>
    public static Role PerfilWeb(this UsuarioAutenticadoDto usuario) =>
        usuario.RolPersonal is { } rol ? rol.AWeb() : Role.Huesped;

    public static ReservationStatus AWeb(this EstadoReserva estado) => estado switch
    {
        EstadoReserva.EnCurso => ReservationStatus.EnCurso,
        EstadoReserva.Finalizada => ReservationStatus.Finalizada,
        EstadoReserva.Cancelada => ReservationStatus.Cancelada,
        _ => ReservationStatus.Confirmada,
    };

    public static ServiceRequestStatus AWeb(this EstadoSolicitud estado) => estado switch
    {
        EstadoSolicitud.Pendiente => ServiceRequestStatus.Pendiente,
        EstadoSolicitud.EnProceso => ServiceRequestStatus.EnProceso,
        _ => ServiceRequestStatus.Completado,
    };

    /* ─── DTOs ─── */

    public static RoomDto AWeb(this HabitacionDto habitacion) => new()
    {
        Id = habitacion.Id,
        Number = habitacion.Numero,
        Type = habitacion.Tipo.AWeb(),
        Status = habitacion.Estado.AWeb(),
        Floor = habitacion.Piso,
        Capacity = habitacion.Capacidad,
        Price = habitacion.PrecioNoche,
        GuestName = habitacion.HuespedActual,
        CheckIn = habitacion.FechaEntrada is { } entrada ? Formato.FechaCorta(entrada) : null,
        CheckOut = habitacion.FechaSalida is { } salida ? Formato.FechaCorta(salida) : null,
        LastUpdated = Ui.Stamp(habitacion.UltimaActualizacion),
        MaintenanceNote = habitacion.NotaMantenimiento,
    };

    public static TourDto AWeb(this TourResumenDto tour) => new()
    {
        Id = tour.Id,
        Name = tour.Nombre,
        Destination = tour.Destino,
        Date = Formato.FechaCorta(tour.Fecha),
        Time = Formato.Hora12(tour.HorarioInicio),
        Capacity = tour.CupoMaximo,
        Booked = tour.CuposReservados,
        Guide = tour.NombreGuia ?? string.Empty,
        Vehicle = tour.NombreVehiculo ?? string.Empty,
        Price = tour.PrecioPorPersona,
    };

    public static ConsumptionDto AWeb(this ConsumoDto consumo) => new()
    {
        Id = consumo.Id,
        RoomNumber = consumo.NumeroHabitacion,
        GuestName = consumo.NombreHuesped,
        Service = consumo.Cantidad > 1 ? $"{consumo.NombreServicio} x{consumo.Cantidad}" : consumo.NombreServicio,
        Amount = consumo.Subtotal,
        Date = Formato.FechaCorta(DateOnly.FromDateTime(consumo.FechaHora)),
        Time = Formato.Hora(consumo.FechaHora),
    };

    public static BillTourDto AWeb(this ReservaTourDto reserva) =>
        new(reserva.NombreTour, Formato.FechaCorta(reserva.Fecha), reserva.NumeroPersonas, reserva.ValorTotalTour);

    public static CheckInOutItemDto AWeb(this MovimientoDto movimiento) => new()
    {
        Id = movimiento.Codigo,
        GuestName = movimiento.Huesped,
        RoomNumber = movimiento.NumeroHabitacion,
        Nights = movimiento.Noches,
        CheckIn = Formato.FechaCorta(movimiento.FechaEntrada),
        CheckOut = Formato.FechaCorta(movimiento.FechaSalida),
    };

    public static GuestReservationDto AWeb(this ReservaHuespedDto reserva) => new()
    {
        Id = reserva.Codigo,
        Kind = reserva.EsTour ? ReservationKind.Tour : ReservationKind.Habitacion,
        Status = reserva.Estado.AWeb(),
        RoomType = reserva.TipoHabitacion?.AWeb(),
        RoomNumber = reserva.NumeroHabitacion,
        CheckIn = reserva.FechaEntrada is { } entrada ? Formato.FechaCorta(entrada) : null,
        CheckOut = reserva.FechaSalida is { } salida ? Formato.FechaCorta(salida) : null,
        Nights = reserva.Noches,
        PricePerNight = reserva.PrecioNoche,
        TourName = reserva.NombreTour,
        TourDate = reserva.FechaTour is { } fechaTour ? Formato.FechaCorta(fechaTour) : null,
        Persons = reserva.Personas,
        TourPrice = reserva.PrecioPorPersona,
        Cancellable = reserva.Cancelable,
        LateCancel = reserva.CancelacionTardia,
        CancelReason = reserva.MotivoNoCancelable,
    };

    public static ServiceCatalogItemDto AWeb(this ServicioAdicionalDto servicio) => new()
    {
        Id = servicio.Id,
        Name = servicio.Nombre,
        Category = servicio.Categoria,
        Description = servicio.Descripcion,
        Price = servicio.TienePrecioFijo ? servicio.PrecioUnitario : null,
        Active = servicio.Activo,
    };

    public static ServiceRequestDto AWeb(this SolicitudServicioDto solicitud) => new()
    {
        Id = solicitud.Id,
        ServiceId = solicitud.ServicioId,
        ServiceName = solicitud.NombreServicio,
        RoomNumber = solicitud.NumeroHabitacion,
        Status = solicitud.Estado.AWeb(),
        RequestedAt = Ui.Stamp(solicitud.FechaSolicitud),
    };

    public static StaffUserDto AWeb(this EmpleadoDto empleado) => new()
    {
        Id = empleado.Id,
        Name = empleado.Nombre,
        Email = empleado.Correo,
        Role = empleado.Rol.AWeb(),
        Active = empleado.Activo,
        Username = empleado.Usuario,
        // La contrasena nunca sale del backend: en la base solo existe su hash.
    };
}
