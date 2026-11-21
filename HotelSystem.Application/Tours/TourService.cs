using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.Comun;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Application.Tours;

public sealed class TourService(
    ITourRepository tours,
    IPersonaRepository personas,
    IReservaHabitacionRepository reservas,
    ICuentaRepository cuentas,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    Bitacora bitacora,
    OpcionesHotel opciones) : ITourService
{
    /* ─── Consultas ─── */

    public IReadOnlyList<TourResumenDto> Listar() =>
        tours.Listar().Select(TourResumenDto.Desde).ToList();

    public TourResumenDto? Obtener(int id)
    {
        var tour = tours.ObtenerPorId(id);
        return tour is null ? null : TourResumenDto.Desde(tour);
    }

    public RecursosDto ListarRecursos(int? tourId = null)
    {
        var todos = tours.Listar();
        var tour = tourId is null ? null : todos.FirstOrDefault(t => t.Id == tourId.Value);
        var cruzados = tour is null
            ? new List<Tour>()
            : todos.Where(t => t.Id != tour.Id && t.SeCruzaCon(tour)).ToList();

        var guias = tours.ListarGuias()
            .Select(g => new GuiaDto(g.Id, g.Nombre, cruzados.Any(t => t.GuiaId == g.Id)))
            .ToList();
        var vehiculos = tours.ListarVehiculos()
            .Select(v => new VehiculoDto(v.Id, v.Nombre, v.Placa, v.Modelo, v.CapacidadPasajeros, cruzados.Any(t => t.VehiculoId == v.Id)))
            .ToList();

        return new RecursosDto(guias, vehiculos);
    }

    public IReadOnlyList<ReservaTourDto> ReservasDeHuesped(int huespedId) =>
        tours.ListarReservasPorHuesped(huespedId).Select(ReservaTourDto.Desde).ToList();

    public IReadOnlyList<ReservaTourDto> ReservasDeCuenta(int cuentaId) =>
        tours.ListarReservasPorCuenta(cuentaId)
            .Where(r => r.EstaVigente)
            .Select(ReservaTourDto.Desde)
            .ToList();

    /* ─── HU-10: cupos ─── */

    public TourResumenDto BloquearCupos(int tourId, int cantidad, int empleadoId)
    {
        return unidadDeTrabajo.EnTransaccion(() =>
        {
            var tour = Buscar(tourId);
            tour.BloquearCupos(cantidad); // RN-05, RN-11

            bitacora.Registrar(empleadoId, TipoOperacion.AjusteCupos, $"{tour.Nombre}: {cantidad} cupo(s) bloqueado(s)");
            unidadDeTrabajo.GuardarCambios();
            return TourResumenDto.Desde(tour);
        });
    }

    public TourResumenDto LiberarCupos(int tourId, int cantidad, int empleadoId)
    {
        return unidadDeTrabajo.EnTransaccion(() =>
        {
            var tour = Buscar(tourId);

            // Los cupos que respaldan reservas de huéspedes solo se liberan cancelando la reserva.
            var deHuespedes = tours.ListarReservasVigentesPorTour(tour.Id).Sum(r => r.NumeroPersonas);
            if (tour.CuposReservados - cantidad < deHuespedes)
                throw new ReglaNegocioException($"No se pueden liberar esos cupos: {deHuespedes} pertenecen a reservas confirmadas de huespedes (RN-05).", "RN-05");

            tour.LiberarCupos(cantidad);

            bitacora.Registrar(empleadoId, TipoOperacion.AjusteCupos, $"{tour.Nombre}: {cantidad} cupo(s) liberado(s)");
            unidadDeTrabajo.GuardarCambios();
            return TourResumenDto.Desde(tour);
        });
    }

    /* ─── HU-10: guías y vehículos ─── */

    public TourResumenDto AsignarRecursos(int tourId, int guiaId, int vehiculoId, int empleadoId)
    {
        return unidadDeTrabajo.EnTransaccion(() =>
        {
            var tour = Buscar(tourId);
            var guia = tours.ObtenerGuia(guiaId)
                ?? throw new EntidadNoEncontradaException("El guia no existe.");
            var vehiculo = tours.ObtenerVehiculo(vehiculoId)
                ?? throw new EntidadNoEncontradaException("El vehiculo no existe.");

            // RN-03: un guía o un vehículo no puede estar en dos tours cuyos horarios se cruzan.
            var cruzados = tours.Listar().Where(t => t.Id != tour.Id && t.SeCruzaCon(tour)).ToList();
            if (cruzados.Any(t => t.GuiaId == guia.Id))
                throw new ReglaNegocioException($"{guia.Nombre} ya esta asignado a otro tour en el mismo horario (RN-03 - HU-10 Esc.4).", "RN-03");
            if (cruzados.Any(t => t.VehiculoId == vehiculo.Id))
                throw new ReglaNegocioException($"{vehiculo.Nombre} ya esta asignado a otro tour en el mismo horario (RN-03 - HU-10 Esc.4).", "RN-03");

            tour.AsignarRecursos(guia, vehiculo);

            bitacora.Registrar(empleadoId, TipoOperacion.AsignacionRecursos, $"{tour.Nombre}: {guia.Nombre} + {vehiculo.Nombre}");
            unidadDeTrabajo.GuardarCambios();
            return TourResumenDto.Desde(tour);
        });
    }

    public RecursosLiberadosDto LiberarRecursos(int tourId, int empleadoId)
    {
        var tour = Buscar(tourId);
        var liberados = new RecursosLiberadosDto(tour.Nombre, tour.Guia?.Nombre, tour.Vehiculo?.Nombre);

        tour.LiberarRecursos();

        bitacora.Registrar(empleadoId, TipoOperacion.AsignacionRecursos, $"{tour.Nombre}: recursos liberados");
        unidadDeTrabajo.GuardarCambios();
        return liberados;
    }

    /* ─── HU-09: reserva del huésped ─── */

    public ReservaTourDto ReservarTour(SolicitudReservaTour solicitud)
    {
        if (!solicitud.MayorDeEdadConfirmado)
            throw new ReglaNegocioException("Debe confirmar que es mayor de 18 anos (RN-10 - HU-09 Esc.4).", "RN-10");
        if (solicitud.MedioPago is not { } medioPago)
            throw new ReglaNegocioException("Debe registrar un medio de pago o deposito (RN-04 - HU-09 Esc.3).", "RN-04");
        if (solicitud.NumeroPersonas < 1)
            throw new ReglaNegocioException("Indique al menos una persona.");

        // Comprobar cupos y bloquearlos ocurre en una sola transacción serializable (RF-04, RN-05, RNF-07).
        return unidadDeTrabajo.EnTransaccion(() =>
        {
            var huesped = personas.ObtenerHuesped(solicitud.HuespedId)
                ?? throw new EntidadNoEncontradaException("El huesped no existe.");
            if (!huesped.EsMayorDeEdad(reloj.Hoy))
                throw new ReglaNegocioException("El titular de la reserva debe ser mayor de 18 anos (RN-10 - HU-09 Esc.4).", "RN-10");

            var tour = Buscar(solicitud.TourId);
            if (tour.Inicio <= reloj.Ahora)
                throw new ReglaNegocioException($"El tour {tour.Nombre} ya inicio o ya se realizo.");
            if (tours.ListarReservasVigentesPorTour(tour.Id).Any(r => r.HuespedId == huesped.Id))
                throw new ReglaNegocioException($"Ya tiene una reserva para el tour {tour.Nombre}.");
            if (solicitud.NumeroPersonas > tour.CuposDisponibles)
                throw new ReglaNegocioException("No hay cupos suficientes. Seleccione otra fecha o tour (RN-11 - HU-09 Esc.2).", "RN-11");

            // RN-07: el tour no se paga aparte, se carga a la cuenta única de la estancia que cubre su fecha.
            var estancia = reservas.ListarVigentesDeHuesped(huesped.Id)
                .Where(r => r.Cubre(tour.Fecha))
                .OrderBy(r => r.Estado == EstadoReserva.EnCurso ? 0 : 1)
                .ThenBy(r => r.FechaEntrada)
                .FirstOrDefault()
                ?? throw new ReglaNegocioException($"Para reservar este tour necesita una reserva de habitacion vigente el {Formato.FechaConAnio(tour.Fecha)}: el tour se carga a la cuenta de la estancia (RN-07).", "RN-07");

            var cuenta = cuentas.ObtenerPorReserva(estancia.Id);
            if (cuenta is null || !cuenta.EstaAbierta)
                throw new ReglaNegocioException("La cuenta de la estancia esta cerrada: no se pueden cargar tours (RN-07).", "RN-07");

            tour.BloquearCupos(solicitud.NumeroPersonas);
            var reserva = new ReservaTour(tour, huesped, cuenta, solicitud.NumeroPersonas, medioPago, solicitud.DetallePago, reloj.Ahora);

            tours.AgregarReserva(reserva);
            unidadDeTrabajo.GuardarCambios();
            return ReservaTourDto.Desde(reserva);
        });
    }

    public ReservaTourDto CancelarReserva(int reservaTourId, int huespedId)
    {
        return unidadDeTrabajo.EnTransaccion(() =>
        {
            var reserva = tours.ObtenerReserva(reservaTourId);
            if (reserva is null || reserva.HuespedId != huespedId)
                throw new EntidadNoEncontradaException("La reserva no existe.");
            if (!reserva.EstaVigente)
                throw new ReglaNegocioException($"La reserva {reserva.Codigo} ya esta cancelada.");

            var ahora = reloj.Ahora;
            if (ahora >= reserva.Tour.Inicio)
                throw new ReglaNegocioException("El tour ya se realizo.");
            if (ahora > reserva.Tour.Inicio.AddHours(-opciones.HorasCancelacionTour))
                throw new ReglaNegocioException($"No se permiten cancelaciones con menos de {opciones.HorasCancelacionTour}h de anticipacion.");

            reserva.Cancelar(ahora);
            reserva.Tour.LiberarCupos(reserva.NumeroPersonas);

            unidadDeTrabajo.GuardarCambios();
            return ReservaTourDto.Desde(reserva);
        });
    }

    private Tour Buscar(int tourId) =>
        tours.ObtenerPorId(tourId) ?? throw new EntidadNoEncontradaException("El tour no existe.");
}
