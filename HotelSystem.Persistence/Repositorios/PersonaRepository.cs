using HotelSystem.Application.Abstracciones;
using HotelSystem.Domain.Entidades;

namespace HotelSystem.Persistence.Repositorios;

public sealed class PersonaRepository(HotelDbContext db) : IPersonaRepository
{
    public Persona? BuscarPorUsuario(string usuario) =>
        db.Personas.FirstOrDefault(p => p.Usuario == usuario);

    public bool ExisteUsuario(string usuario) =>
        db.Personas.Any(p => p.Usuario == usuario);

    public bool ExisteCorreo(string correo) =>
        db.Personas.Any(p => p.Correo == correo);

    public Huesped? ObtenerHuesped(int id) =>
        db.Huespedes.FirstOrDefault(h => h.Id == id);

    public Empleado? ObtenerEmpleado(int id) =>
        db.Empleados.FirstOrDefault(e => e.Id == id);

    public IReadOnlyList<Empleado> ListarEmpleados() =>
        db.Empleados.ToList().OrderBy(e => e.Id).ToList();

    public void Agregar(Persona persona) => db.Personas.Add(persona);
}
