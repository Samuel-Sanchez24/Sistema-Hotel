using HotelSystem.Application.Abstracciones;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Application.Usuarios;

public sealed class AuthService(IPersonaRepository personas, IReservaHabitacionRepository reservas, IHasherContrasenas hasher) : IAuthService
{
    public const string MensajeCredencialesInvalidas = "Credenciales incorrectas. Verifique los datos ingresados. (HU-02 Esc.2)";

    public const string MensajeCuentaDesactivada = "Cuenta desactivada. Comuniquese con administracion para reactivar su acceso. (HU-02 Esc.3)";

    public UsuarioAutenticadoDto IniciarSesion(string usuario, string contrasena)
    {
        if (string.IsNullOrWhiteSpace(usuario) || string.IsNullOrEmpty(contrasena))
            throw new ReglaNegocioException(MensajeCredencialesInvalidas);

        var persona = personas.BuscarPorUsuario(usuario.Trim().ToLowerInvariant());
        if (persona is null)
            throw new ReglaNegocioException(MensajeCredencialesInvalidas);
        if (!persona.Activo)
            throw new ReglaNegocioException(MensajeCuentaDesactivada);
        if (!hasher.Verificar(contrasena, persona.ClaveHash))
            throw new ReglaNegocioException(MensajeCredencialesInvalidas);

        if (persona is Empleado empleado)
        {
            return new UsuarioAutenticadoDto(
                empleado.Id, empleado.Nombre, empleado.Correo, empleado.Usuario,
                empleado.RolNombre, EsHuesped: false, RolPersonal: empleado.Rol, NumeroHabitacion: null);
        }

        // Si el huésped tiene una estancia en curso se devuelve su número de habitación.
        var estancia = reservas.ListarVigentesDeHuesped(persona.Id)
            .FirstOrDefault(r => r.Estado == EstadoReserva.EnCurso);

        return new UsuarioAutenticadoDto(
            persona.Id, persona.Nombre, persona.Correo, persona.Usuario,
            persona.RolNombre, EsHuesped: true, RolPersonal: null, NumeroHabitacion: estancia?.Habitacion.Numero);
    }
}
