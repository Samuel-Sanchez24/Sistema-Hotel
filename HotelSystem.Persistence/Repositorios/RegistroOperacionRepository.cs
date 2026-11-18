using HotelSystem.Application.Abstracciones;
using HotelSystem.Domain.Entidades;

namespace HotelSystem.Persistence.Repositorios;

public sealed class RegistroOperacionRepository(HotelDbContext db) : IRegistroOperacionRepository
{
    public void Agregar(RegistroOperacion registro) => db.Operaciones.Add(registro);

    public IReadOnlyList<RegistroOperacion> ListarEntre(DateTime desde, DateTime hastaExclusivo) =>
        db.Operaciones
            .Where(o => o.FechaHora >= desde && o.FechaHora < hastaExclusivo)
            .ToList();
}
