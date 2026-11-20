using HotelSystem.Application.Abstracciones;
using HotelSystem.Domain.Entidades;
using Microsoft.EntityFrameworkCore;

namespace HotelSystem.Persistence.Repositorios;

public sealed class ServicioAdicionalRepository(HotelDbContext db) : IServicioAdicionalRepository
{
    private IQueryable<SolicitudServicio> SolicitudesCompletas =>
        db.Solicitudes
            .Include(s => s.Servicio)
            .Include(s => s.Cuenta).ThenInclude(c => c.Reserva).ThenInclude(r => r.Habitacion)
            .Include(s => s.Cuenta).ThenInclude(c => c.Reserva).ThenInclude(r => r.Huesped);

    public IReadOnlyList<ServicioAdicional> Listar() =>
        db.Servicios
            .ToList()
            .OrderBy(s => s.Id)
            .ToList();

    public ServicioAdicional? ObtenerPorId(int id) =>
        db.Servicios.FirstOrDefault(s => s.Id == id);

    public ServicioAdicional? ObtenerPorNombre(string nombre) =>
        db.Servicios.FirstOrDefault(s => s.Nombre == nombre);

    public SolicitudServicio? ObtenerSolicitud(int id) =>
        SolicitudesCompletas.FirstOrDefault(s => s.Id == id);

    public IReadOnlyList<SolicitudServicio> ListarSolicitudes() =>
        SolicitudesCompletas
            .ToList()
            .OrderByDescending(s => s.FechaSolicitud)
            .ToList();

    public IReadOnlyList<SolicitudServicio> ListarSolicitudesPorCuenta(int cuentaId) =>
        SolicitudesCompletas
            .Where(s => s.CuentaId == cuentaId)
            .ToList()
            .OrderByDescending(s => s.FechaSolicitud)
            .ToList();

    public void AgregarSolicitud(SolicitudServicio solicitud) => db.Solicitudes.Add(solicitud);
}
