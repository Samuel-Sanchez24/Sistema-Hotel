using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.Comun;
using HotelSystem.Application.Housekeeping;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Application.Reservas;

public sealed class ReservaService(
    IReservaHabitacionRepository reservas,
    IHabitacionRepository habitaciones,
    IPersonaRepository personas,
    ICuentaRepository cuentas,
    ITourRepository tours,
    IPoliticaCancelacion politica,
    IServicioCorreo correo,
    SuscriptorHabitaciones suscriptor,
    IUnidadDeTrabajo unidadDeTrabajo,
    IReloj reloj,
    Bitacora bitacora,
    OpcionesHotel opciones) : IReservaService
{
    /* ─── HU-03: buscar y reservar ─── */

    public IReadOnlyList<TipoHabitacionDto> TiposDeHabitacion() =>
        habitaciones.Listar()
            .GroupBy(h => h.Tipo)
            .OrderBy(g => g.Key)
            .Select(g => new TipoHabitacionDto(g.Key, g.Min(h => h.PrecioNoche), g.Max(h => h.Capacidad), g.Count()))
            .ToList();

    public IReadOnlyList<HabitacionDto> BuscarDisponibles(TipoHabitacion tipo, DateOnly fechaEntrada, DateOnly fechaSalida, int cantidadHuespedes)
    {
        ValidarFechas(fechaEntrada, fechaSalida);
        if (cantidadHuespedes < 1)
            throw new ReglaNegocioException("Indique al menos un huesped.");

        // RN-01: una habitación con una reserva activa que comparta alguna noche no se ofrece.
        var ocupadas = reservas.ListarActivasQueSeCruzan(fechaEntrada, fechaSalida)
            .Select(r => r.HabitacionId)
            .ToHashSet();

        return habitaciones.Listar()
            .Where(h => h.Tipo == tipo
                && h.Estado != EstadoHabitacion.Mantenimiento
                && h.AdmiteHuespedes(cantidadHuespedes)
                && !ocupadas.Contains(h.Id))
            .Select(h => HabitacionDto.Desde(h))
            .ToList();
    }

    public ReservaConfirmadaDto ReservarHabitacion(SolicitudReservaHabitacion solicitud)
    {
        if (!solicitud.MayorDeEdadConfirmado)
            throw new ReglaNegocioException("Debe confirmar que es mayor de 18 anos (RN-10 - HU-03 Esc.4).", "RN-10");
        if (solicitud.MedioPago is not { } medioPago)
            throw new ReglaNegocioException("Debe registrar un medio de pago para confirmar la reserva (RN-04 - HU-03 Esc.3).", "RN-04");
        ValidarFechas(solicitud.FechaEntrada, solicitud.FechaSalida);

        // La comprobación de disponibilidad y el guardado van en la misma transacción
        // serializable: dos huéspedes no pueden quedarse con la misma habitación (RN-01, RNF-07).
        var reserva = unidadDeTrabajo.EnTransaccion(() =>
        {
            var huesped = personas.ObtenerHuesped(solicitud.HuespedId)
                ?? throw new EntidadNoEncontradaException("El huesped no existe.");
            if (!huesped.EsMayorDeEdad(reloj.Hoy))
                throw new ReglaNegocioException("El titular de la reserva debe ser mayor de 18 anos (RN-10 - HU-03 Esc.4).", "RN-10");

            var habitacion = habitaciones.ObtenerPorId(solicitud.HabitacionId)
                ?? throw new EntidadNoEncontradaException("La habitacion no existe.");
            if (habitacion.Estado == EstadoHabitacion.Mantenimiento)
                throw new ReglaNegocioException($"La habitacion {habitacion.Numero} esta en mantenimiento y no se puede reservar (RN-02).", "RN-02");
            if (reservas.ExisteCruce(habitacion.Id, solicitud.FechaEntrada, solicitud.FechaSalida))
                throw new ReglaNegocioException($"La habitacion {habitacion.Numero} ya esta reservada en esas fechas. Elija otra habitacion o cambie las fechas (RN-01).", "RN-01");

            // El constructor valida la capacidad (RN-11).
            var nueva = new ReservaHabitacion(huesped, habitacion, solicitud.FechaEntrada, solicitud.FechaSalida, solicitud.CantidadHuespedes, reloj.Ahora);
            nueva.Confirmar(medioPago, solicitud.DetallePago);

            reservas.Agregar(nueva);
            cuentas.Agregar(new Cuenta(nueva, reloj.Ahora));
            unidadDeTrabajo.GuardarCambios();
            return nueva;
        });

        // HU-04: el correo se envía con la reserva ya guardada. Si falla, la reserva sigue confirmada.
        var limiteSinCargo = politica.Evaluar(reserva, reloj.Ahora).LimiteSinCargo;
        var correoEnviado = correo.Enviar(PlantillasCorreo.ConfirmacionReserva(reserva, limiteSinCargo));

        return new ReservaConfirmadaDto(
            reserva.Id,
            reserva.Codigo,
            reserva.Habitacion.Numero,
            reserva.Habitacion.Tipo,
            reserva.FechaEntrada,
            reserva.FechaSalida,
            reserva.Noches,
            reserva.PrecioNoche,
            reserva.ValorTotalAlojamiento,
            reserva.Huesped.Correo,
            correoEnviado);
    }

    /* ─── HU-03: mis reservas y cancelación ─── */

    public IReadOnlyList<ReservaHuespedDto> ReservasDeHuesped(int huespedId)
    {
        var ahora = reloj.Ahora;
        var deHabitacion = reservas.ListarPorHuesped(huespedId).Select(r => FilaHabitacion(r, ahora));
        var deTour = tours.ListarReservasPorHuesped(huespedId).Select(r => FilaTour(r, ahora));

        return deHabitacion.Concat(deTour)
            .OrderBy(r => Orden(r.Estado))
            .ThenBy(r => r.FechaEntrada ?? r.FechaTour)
            .ToList();
    }

    public CancelacionDto CancelarReserva(int reservaId, int huespedId)
    {
        return unidadDeTrabajo.EnTransaccion(() =>
        {
            var reserva = reservas.ObtenerPorId(reservaId);
            if (reserva is null || reserva.HuespedId != huespedId)
                throw new EntidadNoEncontradaException("La reserva no existe.");

            switch (reserva.Estado)
            {
                case EstadoReserva.Cancelada:
                    throw new ReglaNegocioException($"La reserva {reserva.Codigo} ya esta cancelada.");
                case EstadoReserva.EnCurso:
                    throw new ReglaNegocioException("El check-in ya fue registrado. No es posible cancelar una estancia en curso.");
                case EstadoReserva.Finalizada:
                    throw new ReglaNegocioException("La estancia ya finalizo.");
            }

            var ahora = reloj.Ahora;
            var evaluacion = politica.Evaluar(reserva, ahora); // RN-09

            // Los tours cargados a la cuenta de esta estancia se cancelan con ella y devuelven sus cupos.
            var cuenta = cuentas.ObtenerPorReserva(reserva.Id);
            if (cuenta is not null)
            {
                foreach (var reservaTour in cuenta.ReservasTour.Where(t => t.EstaVigente).ToList())
                {
                    reservaTour.Cancelar(ahora);
                    reservaTour.Tour.LiberarCupos(reservaTour.NumeroPersonas);
                }

                if (cuenta.EstaAbierta) cuenta.Anular(ahora);
            }

            reserva.Cancelar(evaluacion.Cargo, ahora);
            unidadDeTrabajo.GuardarCambios();

            return new CancelacionDto(reserva.Codigo, evaluacion.EsTardia, evaluacion.Cargo);
        });
    }

    /* ─── HU-06: check-in y salida ─── */

    public IReadOnlyList<MovimientoDto> LlegadasDeHoy() =>
        reservas.ListarLlegadas(reloj.Hoy).Select(MovimientoDto.Desde).ToList();

    public IReadOnlyList<MovimientoDto> SalidasDeHoy() =>
        reservas.ListarSalidasHasta(reloj.Hoy).Select(MovimientoDto.Desde).ToList();

    public MovimientoDto RegistrarCheckIn(int reservaId, int empleadoId)
    {
        var reserva = reservas.ObtenerPorId(reservaId)
            ?? throw new EntidadNoEncontradaException("La reserva no existe.");

        if (reserva.Estado != EstadoReserva.Confirmada)
            throw new ReglaNegocioException($"La reserva {reserva.Codigo} no esta pendiente de check-in.");
        if (reserva.FechaEntrada > reloj.Hoy)
            throw new ReglaNegocioException($"El check-in de {reserva.Codigo} solo se puede registrar desde el {Formato.FechaConAnio(reserva.FechaEntrada)}.");

        var ahora = reloj.Ahora;

        // State: solo una habitación Disponible admite check-in (RN-02); si no, se lanza la excepción
        // antes de tocar la reserva. Observer: los paneles se enteran del cambio a Ocupada.
        suscriptor.Suscribir(reserva.Habitacion);
        reserva.Habitacion.Ocupar(ahora);
        reserva.RegistrarCheckIn(ahora);

        bitacora.Registrar(empleadoId, TipoOperacion.CheckIn, $"Check-in {reserva.Codigo} - Hab. {reserva.Habitacion.Numero}");
        unidadDeTrabajo.GuardarCambios();

        return MovimientoDto.Desde(reserva);
    }

    public MovimientoDto RegistrarSalida(int reservaId, int empleadoId)
    {
        var reserva = reservas.ObtenerPorId(reservaId)
            ?? throw new EntidadNoEncontradaException("La reserva no existe.");

        if (reserva.Estado != EstadoReserva.EnCurso)
            throw new ReglaNegocioException($"La reserva {reserva.Codigo} no tiene una estancia en curso.");

        var ahora = reloj.Ahora;

        // State: Ocupada -> EnLimpieza. Observer: housekeeping la recibe en su cola de limpieza.
        suscriptor.Suscribir(reserva.Habitacion);
        reserva.Habitacion.Liberar(ahora);
        reserva.RegistrarSalida(ahora);

        bitacora.Registrar(empleadoId, TipoOperacion.CheckOut, $"Check-out {reserva.Codigo} - Hab. {reserva.Habitacion.Numero}");
        unidadDeTrabajo.GuardarCambios();

        return MovimientoDto.Desde(reserva);
    }

    /* ─── Auxiliares ─── */

    private void ValidarFechas(DateOnly entrada, DateOnly salida)
    {
        if (entrada < reloj.Hoy)
            throw new ReglaNegocioException("La fecha de entrada no puede ser anterior a hoy.");
        if (salida <= entrada)
            throw new ReglaNegocioException("La fecha de salida debe ser posterior a la fecha de entrada.");
    }

    private ReservaHuespedDto FilaHabitacion(ReservaHabitacion reserva, DateTime ahora)
    {
        var cancelable = reserva.Estado is EstadoReserva.Solicitada or EstadoReserva.Confirmada;
        var evaluacion = cancelable ? politica.Evaluar(reserva, ahora) : null;

        var motivo = reserva.Estado switch
        {
            EstadoReserva.EnCurso => "El check-in ya fue registrado. No es posible cancelar una estancia en curso.",
            EstadoReserva.Finalizada => "La estancia ya finalizo.",
            _ => null,
        };

        return new ReservaHuespedDto(
            Codigo: reserva.Codigo,
            EsTour: false,
            Estado: reserva.Estado,
            TipoHabitacion: reserva.Habitacion.Tipo,
            NumeroHabitacion: reserva.Habitacion.Numero,
            FechaEntrada: reserva.FechaEntrada,
            FechaSalida: reserva.FechaSalida,
            Noches: reserva.Noches,
            PrecioNoche: reserva.PrecioNoche,
            NombreTour: null,
            FechaTour: null,
            Personas: null,
            PrecioPorPersona: null,
            Total: reserva.ValorTotalAlojamiento,
            Cancelable: cancelable,
            CancelacionTardia: evaluacion?.EsTardia ?? false,
            CargoCancelacion: evaluacion?.Cargo ?? reserva.CargoCancelacion,
            MotivoNoCancelable: motivo);
    }

    private ReservaHuespedDto FilaTour(ReservaTour reserva, DateTime ahora)
    {
        var tour = reserva.Tour;
        var limite = tour.Inicio.AddHours(-opciones.HorasCancelacionTour);
        var cancelable = reserva.EstaVigente && ahora <= limite;

        string? motivo = null;
        if (reserva.EstaVigente && !cancelable)
        {
            motivo = ahora >= tour.Inicio
                ? "El tour ya se realizo."
                : $"El tour es en menos de {opciones.HorasCancelacionTour} horas. No se permiten cancelaciones con menos de {opciones.HorasCancelacionTour}h de anticipacion.";
        }

        return new ReservaHuespedDto(
            Codigo: reserva.Codigo,
            EsTour: true,
            Estado: reserva.Estado,
            TipoHabitacion: null,
            NumeroHabitacion: null,
            FechaEntrada: null,
            FechaSalida: null,
            Noches: null,
            PrecioNoche: null,
            NombreTour: tour.Nombre,
            FechaTour: tour.Fecha,
            Personas: reserva.NumeroPersonas,
            PrecioPorPersona: reserva.PrecioPorPersona,
            Total: reserva.CalcularTotal(),
            Cancelable: cancelable,
            CancelacionTardia: false,
            CargoCancelacion: 0m,
            MotivoNoCancelable: motivo);
    }

    /// <summary>En curso primero, luego las confirmadas, y al final las finalizadas y canceladas.</summary>
    private static int Orden(EstadoReserva estado) => estado switch
    {
        EstadoReserva.EnCurso => 0,
        EstadoReserva.Solicitada or EstadoReserva.Confirmada => 1,
        EstadoReserva.Finalizada => 2,
        _ => 3,
    };
}
