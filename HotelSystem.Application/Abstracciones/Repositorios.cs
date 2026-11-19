using HotelSystem.Domain.Entidades;

namespace HotelSystem.Application.Abstracciones;

// Puertos de persistencia. La capa Application define QUÉ datos necesita y la capa
// Persistence decide CÓMO obtenerlos (Entity Framework Core). Es la inversión de
// dependencias: los servicios no conocen la base de datos.
//
// Las entidades que devuelven quedan bajo seguimiento: basta modificarlas y llamar a
// IUnidadDeTrabajo.GuardarCambios().

public interface IPersonaRepository
{
    /// <summary>Busca por nombre de usuario (en minúsculas).</summary>
    Persona? BuscarPorUsuario(string usuario);

    bool ExisteUsuario(string usuario);

    bool ExisteCorreo(string correo);

    Huesped? ObtenerHuesped(int id);

    Empleado? ObtenerEmpleado(int id);

    IReadOnlyList<Empleado> ListarEmpleados();

    void Agregar(Persona persona);
}

public interface IHabitacionRepository
{
    /// <summary>Todas las habitaciones, ordenadas por número.</summary>
    IReadOnlyList<Habitacion> Listar();

    Habitacion? ObtenerPorId(int id);

    Habitacion? ObtenerPorNumero(string numero);
}

public interface IReservaHabitacionRepository
{
    /// <summary>Incluye habitación y huésped.</summary>
    ReservaHabitacion? ObtenerPorId(int id);

    /// <summary>Todas las reservas del huésped, con su habitación.</summary>
    IReadOnlyList<ReservaHabitacion> ListarPorHuesped(int huespedId);

    /// <summary>Reservas confirmadas o en curso del huésped, con su habitación.</summary>
    IReadOnlyList<ReservaHabitacion> ListarVigentesDeHuesped(int huespedId);

    /// <summary>Estancias con check-in hecho y sin check-out, con habitación y huésped.</summary>
    IReadOnlyList<ReservaHabitacion> ListarEnCurso();

    /// <summary>Reservas activas (solicitadas, confirmadas o en curso) que comparten alguna noche con el rango.</summary>
    IReadOnlyList<ReservaHabitacion> ListarActivasQueSeCruzan(DateOnly entrada, DateOnly salida);

    /// <summary>RN-01: ¿la habitación ya tiene una reserva activa en esas fechas?</summary>
    bool ExisteCruce(int habitacionId, DateOnly entrada, DateOnly salida);

    /// <summary>
    /// Reservas confirmadas que esperan check-in en la fecha: su entrada es ese día o anterior
    /// (un huésped puede llegar tarde) y su salida es posterior. Con habitación y huésped.
    /// </summary>
    IReadOnlyList<ReservaHabitacion> ListarLlegadas(DateOnly fecha);

    /// <summary>Estancias en curso cuya salida es la fecha o anterior, con habitación y huésped.</summary>
    IReadOnlyList<ReservaHabitacion> ListarSalidasHasta(DateOnly fecha);

    /// <summary>Reservas no canceladas con alguna noche dentro del rango (para reportes), con habitación.</summary>
    IReadOnlyList<ReservaHabitacion> ListarNoCanceladasEnRango(DateOnly desde, DateOnly hasta);

    /// <summary>Reservas canceladas en el intervalo (para los cargos de la RN-09).</summary>
    IReadOnlyList<ReservaHabitacion> ListarCanceladasEntre(DateTime desde, DateTime hastaExclusivo);

    void Agregar(ReservaHabitacion reserva);
}

/// <summary>
/// Las cuentas se devuelven completas: reserva (con habitación y huésped), consumos
/// (con su servicio) y tours (con su tour).
/// </summary>
public interface ICuentaRepository
{
    Cuenta? ObtenerPorId(int id);

    Cuenta? ObtenerPorReserva(int reservaHabitacionId);

    /// <summary>Cuenta abierta de la estancia en curso en esa habitación.</summary>
    Cuenta? ObtenerAbiertaPorHabitacion(int habitacionId);

    /// <summary>Cuenta abierta de la estancia en curso del huésped.</summary>
    Cuenta? ObtenerAbiertaPorHuesped(int huespedId);

    /// <summary>Todos los consumos, del más reciente al más antiguo.</summary>
    IReadOnlyList<ConsumoServicio> ListarConsumos();

    IReadOnlyList<ConsumoServicio> ListarConsumosEntre(DateTime desde, DateTime hastaExclusivo);

    void Agregar(Cuenta cuenta);

    void AgregarPrefactura(Prefactura prefactura);
}

public interface ITourRepository
{
    /// <summary>Todos los tours con guía y vehículo, ordenados por fecha y hora.</summary>
    IReadOnlyList<Tour> Listar();

    Tour? ObtenerPorId(int id);

    IReadOnlyList<Guia> ListarGuias();

    IReadOnlyList<Vehiculo> ListarVehiculos();

    Guia? ObtenerGuia(int id);

    Vehiculo? ObtenerVehiculo(int id);

    /// <summary>Incluye el tour.</summary>
    ReservaTour? ObtenerReserva(int id);

    IReadOnlyList<ReservaTour> ListarReservasPorHuesped(int huespedId);

    IReadOnlyList<ReservaTour> ListarReservasPorCuenta(int cuentaId);

    IReadOnlyList<ReservaTour> ListarReservasVigentesPorTour(int tourId);

    /// <summary>Reservas no canceladas de tours cuya fecha cae en el rango (para reportes).</summary>
    IReadOnlyList<ReservaTour> ListarReservasVigentesEnRango(DateOnly desde, DateOnly hasta);

    void AgregarReserva(ReservaTour reserva);
}

public interface IServicioAdicionalRepository
{
    IReadOnlyList<ServicioAdicional> Listar();

    ServicioAdicional? ObtenerPorId(int id);

    ServicioAdicional? ObtenerPorNombre(string nombre);

    /// <summary>Incluye servicio y cuenta (con reserva, habitación y huésped).</summary>
    SolicitudServicio? ObtenerSolicitud(int id);

    IReadOnlyList<SolicitudServicio> ListarSolicitudes();

    IReadOnlyList<SolicitudServicio> ListarSolicitudesPorCuenta(int cuentaId);

    void AgregarSolicitud(SolicitudServicio solicitud);
}

public interface IRegistroOperacionRepository
{
    void Agregar(RegistroOperacion registro);

    IReadOnlyList<RegistroOperacion> ListarEntre(DateTime desde, DateTime hastaExclusivo);
}
