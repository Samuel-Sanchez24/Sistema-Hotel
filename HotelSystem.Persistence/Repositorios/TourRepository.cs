using HotelSystem.Application.Abstracciones;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using Microsoft.EntityFrameworkCore;

namespace HotelSystem.Persistence.Repositorios;

public sealed class TourRepository(HotelDbContext db) : ITourRepository
{
    private IQueryable<Tour> Completos =>
        db.Tours
            .Include(t => t.Guia)
            .Include(t => t.Vehiculo);

    private IQueryable<ReservaTour> ReservasCompletas =>
        db.ReservasTour
            .Include(r => r.Tour)
            .Include(r => r.Huesped);

    public IReadOnlyList<Tour> Listar() =>
        Completos
            .ToList()
            .OrderBy(t => t.Fecha)
            .ThenBy(t => t.HorarioInicio)
            .ThenBy(t => t.Id)
            .ToList();

    public Tour? ObtenerPorId(int id) =>
        Completos.FirstOrDefault(t => t.Id == id);

    public IReadOnlyList<Guia> ListarGuias() =>
        db.Guias
            .Where(g => g.Activo)
            .ToList()
            .OrderBy(g => g.Id)
            .ToList();

    public IReadOnlyList<Vehiculo> ListarVehiculos() =>
        db.Vehiculos
            .Where(v => v.Activo)
            .ToList()
            .OrderBy(v => v.Id)
            .ToList();

    public Guia? ObtenerGuia(int id) =>
        db.Guias.FirstOrDefault(g => g.Id == id);

    public Vehiculo? ObtenerVehiculo(int id) =>
        db.Vehiculos.FirstOrDefault(v => v.Id == id);

    public ReservaTour? ObtenerReserva(int id) =>
        ReservasCompletas.FirstOrDefault(r => r.Id == id);

    public IReadOnlyList<ReservaTour> ListarReservasPorHuesped(int huespedId) =>
        ReservasCompletas
            .Where(r => r.HuespedId == huespedId)
            .ToList();

    public IReadOnlyList<ReservaTour> ListarReservasPorCuenta(int cuentaId) =>
        ReservasCompletas
            .Where(r => r.CuentaId == cuentaId)
            .ToList();

    public IReadOnlyList<ReservaTour> ListarReservasVigentesPorTour(int tourId) =>
        ReservasCompletas
            .Where(r => r.TourId == tourId && r.Estado != EstadoReserva.Cancelada)
            .ToList();

    public IReadOnlyList<ReservaTour> ListarReservasVigentesEnRango(DateOnly desde, DateOnly hasta) =>
        ReservasCompletas
            .Where(r => r.Estado != EstadoReserva.Cancelada && r.Tour.Fecha >= desde && r.Tour.Fecha <= hasta)
            .ToList();

    public void AgregarReserva(ReservaTour reserva) => db.ReservasTour.Add(reserva);
}
