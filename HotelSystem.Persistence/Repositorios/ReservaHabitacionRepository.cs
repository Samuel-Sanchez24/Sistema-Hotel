using HotelSystem.Application.Abstracciones;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using Microsoft.EntityFrameworkCore;

namespace HotelSystem.Persistence.Repositorios;

public sealed class ReservaHabitacionRepository(HotelDbContext db) : IReservaHabitacionRepository
{
    /// <summary>Reservas con su habitación y su huésped cargados.</summary>
    private IQueryable<ReservaHabitacion> Completas =>
        db.ReservasHabitacion
            .Include(r => r.Habitacion)
            .Include(r => r.Huesped);

    public ReservaHabitacion? ObtenerPorId(int id) =>
        Completas.FirstOrDefault(r => r.Id == id);

    public IReadOnlyList<ReservaHabitacion> ListarPorHuesped(int huespedId) =>
        Completas
            .Where(r => r.HuespedId == huespedId)
            .ToList()
            .OrderBy(r => r.FechaEntrada)
            .ToList();

    public IReadOnlyList<ReservaHabitacion> ListarVigentesDeHuesped(int huespedId) =>
        Completas
            .Where(r => r.HuespedId == huespedId
                && (r.Estado == EstadoReserva.Confirmada || r.Estado == EstadoReserva.EnCurso))
            .ToList()
            .OrderBy(r => r.FechaEntrada)
            .ToList();

    public IReadOnlyList<ReservaHabitacion> ListarEnCurso() =>
        Completas
            .Where(r => r.Estado == EstadoReserva.EnCurso)
            .ToList();

    public IReadOnlyList<ReservaHabitacion> ListarActivasQueSeCruzan(DateOnly entrada, DateOnly salida) =>
        Completas
            .Where(r => (r.Estado == EstadoReserva.Solicitada || r.Estado == EstadoReserva.Confirmada || r.Estado == EstadoReserva.EnCurso)
                && r.FechaEntrada < salida
                && r.FechaSalida > entrada)
            .ToList();

    public bool ExisteCruce(int habitacionId, DateOnly entrada, DateOnly salida) =>
        db.ReservasHabitacion.Any(r => r.HabitacionId == habitacionId
            && (r.Estado == EstadoReserva.Solicitada || r.Estado == EstadoReserva.Confirmada || r.Estado == EstadoReserva.EnCurso)
            && r.FechaEntrada < salida
            && r.FechaSalida > entrada);

    public IReadOnlyList<ReservaHabitacion> ListarLlegadas(DateOnly fecha) =>
        Completas
            .Where(r => r.Estado == EstadoReserva.Confirmada && r.FechaEntrada <= fecha && r.FechaSalida > fecha)
            .ToList()
            .OrderBy(r => r.FechaEntrada)
            .ThenBy(r => r.Id)
            .ToList();

    public IReadOnlyList<ReservaHabitacion> ListarSalidasHasta(DateOnly fecha) =>
        Completas
            .Where(r => r.Estado == EstadoReserva.EnCurso && r.FechaSalida <= fecha)
            .ToList()
            .OrderBy(r => r.FechaSalida)
            .ThenBy(r => r.Id)
            .ToList();

    public IReadOnlyList<ReservaHabitacion> ListarNoCanceladasEnRango(DateOnly desde, DateOnly hasta) =>
        Completas
            .Where(r => r.Estado != EstadoReserva.Cancelada && r.FechaEntrada <= hasta && r.FechaSalida > desde)
            .ToList();

    public IReadOnlyList<ReservaHabitacion> ListarCanceladasEntre(DateTime desde, DateTime hastaExclusivo) =>
        Completas
            .Where(r => r.Estado == EstadoReserva.Cancelada
                && r.FechaCancelacion >= desde
                && r.FechaCancelacion < hastaExclusivo)
            .ToList();

    public void Agregar(ReservaHabitacion reserva) => db.ReservasHabitacion.Add(reserva);
}
