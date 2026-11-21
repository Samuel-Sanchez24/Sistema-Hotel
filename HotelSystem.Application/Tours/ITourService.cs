namespace HotelSystem.Application.Tours;

/// <summary>Tours: reserva del huésped (HU-09) y administración del coordinador (HU-10).</summary>
public interface ITourService
{
    IReadOnlyList<TourResumenDto> Listar();

    TourResumenDto? Obtener(int id);

    /// <summary>
    /// Guías y vehículos. Si se indica un tour, cada recurso viene marcado como ocupado
    /// cuando ya está asignado a otro tour que se cruza de horario con él (RN-03).
    /// </summary>
    RecursosDto ListarRecursos(int? tourId = null);

    /// <summary>El coordinador bloquea cupos manualmente (por ejemplo, una venta en recepción).</summary>
    TourResumenDto BloquearCupos(int tourId, int cantidad, int empleadoId);

    /// <summary>El coordinador libera cupos bloqueados manualmente. No puede liberar los de reservas de huéspedes.</summary>
    TourResumenDto LiberarCupos(int tourId, int cantidad, int empleadoId);

    /// <summary>Asigna guía y vehículo. Rechaza la asignación si alguno se cruza de horario (RN-03).</summary>
    TourResumenDto AsignarRecursos(int tourId, int guiaId, int vehiculoId, int empleadoId);

    RecursosLiberadosDto LiberarRecursos(int tourId, int empleadoId);

    /// <summary>
    /// Reserva cupos para el huésped y los bloquea en el acto (RF-04, RN-05).
    /// Valida RN-04, RN-10 y RN-11, y carga el tour a la cuenta de su estancia (RN-07).
    /// </summary>
    ReservaTourDto ReservarTour(SolicitudReservaTour solicitud);

    /// <summary>Cancela la reserva de un tour y devuelve los cupos.</summary>
    ReservaTourDto CancelarReserva(int reservaTourId, int huespedId);

    IReadOnlyList<ReservaTourDto> ReservasDeHuesped(int huespedId);

    /// <summary>Tours vigentes cargados a una cuenta. Lo usa la fachada de cierre de cuenta.</summary>
    IReadOnlyList<ReservaTourDto> ReservasDeCuenta(int cuentaId);
}
