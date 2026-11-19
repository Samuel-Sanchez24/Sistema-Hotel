using HotelSystem.Application.Abstracciones;
using HotelSystem.Domain.Entidades;

namespace HotelSystem.Persistence.Repositorios;

public sealed class HabitacionRepository(HotelDbContext db) : IHabitacionRepository
{
    public IReadOnlyList<Habitacion> Listar() =>
        db.Habitaciones.ToList()
            .OrderBy(h => h.Piso)
            .ThenBy(h => h.Numero, StringComparer.Ordinal)
            .ToList();

    public Habitacion? ObtenerPorId(int id) =>
        db.Habitaciones.FirstOrDefault(h => h.Id == id);

    public Habitacion? ObtenerPorNumero(string numero) =>
        db.Habitaciones.FirstOrDefault(h => h.Numero == numero);
}
