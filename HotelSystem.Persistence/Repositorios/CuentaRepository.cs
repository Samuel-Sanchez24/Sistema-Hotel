using HotelSystem.Application.Abstracciones;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using Microsoft.EntityFrameworkCore;

namespace HotelSystem.Persistence.Repositorios;

public sealed class CuentaRepository(HotelDbContext db) : ICuentaRepository
{
    /// <summary>Cuentas con todo lo necesario para calcular el monto consolidado (RN-07).</summary>
    private IQueryable<Cuenta> Completas =>
        db.Cuentas
            .Include(c => c.Reserva).ThenInclude(r => r.Habitacion)
            .Include(c => c.Reserva).ThenInclude(r => r.Huesped)
            .Include(c => c.Consumos).ThenInclude(x => x.Servicio)
            .Include(c => c.ReservasTour).ThenInclude(t => t.Tour);

    private IQueryable<ConsumoServicio> ConsumosCompletos =>
        db.Consumos
            .Include(x => x.Servicio)
            .Include(x => x.Cuenta).ThenInclude(c => c.Reserva).ThenInclude(r => r.Habitacion)
            .Include(x => x.Cuenta).ThenInclude(c => c.Reserva).ThenInclude(r => r.Huesped);

    public Cuenta? ObtenerPorId(int id) =>
        Completas.FirstOrDefault(c => c.Id == id);

    public Cuenta? ObtenerPorReserva(int reservaHabitacionId) =>
        Completas.FirstOrDefault(c => c.ReservaHabitacionId == reservaHabitacionId);

    public Cuenta? ObtenerAbiertaPorHabitacion(int habitacionId) =>
        Completas.FirstOrDefault(c => c.Estado == EstadoCuenta.Abierta
            && c.Reserva.HabitacionId == habitacionId
            && c.Reserva.Estado == EstadoReserva.EnCurso);

    public Cuenta? ObtenerAbiertaPorHuesped(int huespedId) =>
        Completas.FirstOrDefault(c => c.Estado == EstadoCuenta.Abierta
            && c.Reserva.HuespedId == huespedId
            && c.Reserva.Estado == EstadoReserva.EnCurso);

    public IReadOnlyList<ConsumoServicio> ListarConsumos() =>
        ConsumosCompletos
            .ToList()
            .OrderByDescending(x => x.FechaHora)
            .ToList();

    public IReadOnlyList<ConsumoServicio> ListarConsumosEntre(DateTime desde, DateTime hastaExclusivo) =>
        ConsumosCompletos
            .Where(x => x.FechaHora >= desde && x.FechaHora < hastaExclusivo)
            .ToList();

    public void Agregar(Cuenta cuenta) => db.Cuentas.Add(cuenta);

    public void AgregarPrefactura(Prefactura prefactura) => db.Prefacturas.Add(prefactura);
}
