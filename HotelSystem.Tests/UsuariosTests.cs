using HotelSystem.Application.Usuarios;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;
using HotelSystem.Tests.Soporte;

namespace HotelSystem.Tests;

/// <summary>Inicio de sesion (HU-02), gestion de cuentas (HU-01, RN-08) y Factory Method.</summary>
public class UsuariosTests : IDisposable
{
    private readonly EntornoPruebas _entorno = new();

    public void Dispose() => _entorno.Dispose();

    [Fact]
    public void Login_HuespedConEstanciaEnCurso_DevuelveSuHabitacion()
    {
        var usuario = _entorno.En<IAuthService, UsuarioAutenticadoDto>(a => a.IniciarSesion("valentina.lagos", "guest2026"));

        Assert.True(usuario.EsHuesped);
        Assert.Null(usuario.RolPersonal);
        Assert.Equal("408", usuario.NumeroHabitacion);
    }

    [Fact]
    public void Login_Empleado_DevuelveSuUnicoRol()
    {
        var usuario = _entorno.En<IAuthService, UsuarioAutenticadoDto>(a => a.IniciarSesion("mlopez", "recep2026"));

        Assert.False(usuario.EsHuesped);
        Assert.Equal(RolEmpleado.Recepcion, usuario.RolPersonal);
    }

    [Fact]
    public void Login_ContrasenaIncorrecta_SeRechaza()
    {
        var error = Assert.Throws<ReglaNegocioException>(() =>
            _entorno.En<IAuthService, UsuarioAutenticadoDto>(a => a.IniciarSesion("mlopez", "otra")));
        Assert.Equal(AuthService.MensajeCredencialesInvalidas, error.Message);
    }

    [Fact]
    public void Login_CuentaDesactivada_SeRechaza()
    {
        var error = Assert.Throws<ReglaNegocioException>(() =>
            _entorno.En<IAuthService, UsuarioAutenticadoDto>(a => a.IniciarSesion("jherrera", "hk2026")));
        Assert.Equal(AuthService.MensajeCuentaDesactivada, error.Message);
    }

    [Fact]
    public void FactoryMethod_CadaCreadorProduceSuTipoDeUsuario()
    {
        // El codigo cliente solo conoce el tipo abstracto CreadorUsuario.
        var huesped = _entorno.En<CreadorHuesped, Persona>(creador => Registrar(creador,
            new DatosNuevoUsuario("Ana Nueva", "ana.nueva@email.com", FechaNacimiento: new DateOnly(1995, 5, 5))));
        var empleado = _entorno.En<CreadorEmpleado, Persona>(creador => Registrar(creador,
            new DatosNuevoUsuario("Beto Nuevo", "beto@hotel.com", Rol: RolEmpleado.Servicios)));

        Assert.IsType<Huesped>(huesped);
        var comoEmpleado = Assert.IsType<Empleado>(empleado);
        Assert.Equal(RolEmpleado.Servicios, comoEmpleado.Rol);
        Assert.Equal("beto", comoEmpleado.Usuario);
    }

    private static Persona Registrar(CreadorUsuario creador, DatosNuevoUsuario datos) => creador.Registrar(datos).Persona;

    [Fact]
    public void CrearEmpleado_EntregaContrasenaTemporalQuePermiteIniciarSesion()
    {
        var admin = _entorno.IdPersona("rsilva");
        var creado = _entorno.En<IUsuarioService, EmpleadoCreadoDto>(u => u.CrearEmpleado("Sara Mejia", "smejia@hotelff.com", RolEmpleado.Recepcion, admin));

        Assert.Equal("smejia", creado.Empleado.Usuario);
        Assert.StartsWith("Hotel-", creado.ContrasenaTemporal);

        var sesion = _entorno.En<IAuthService, UsuarioAutenticadoDto>(a => a.IniciarSesion("smejia", creado.ContrasenaTemporal));
        Assert.Equal(RolEmpleado.Recepcion, sesion.RolPersonal);
    }

    [Fact]
    public void CrearEmpleado_CorreoRepetido_SeRechaza()
    {
        var admin = _entorno.IdPersona("rsilva");

        Assert.Throws<ReglaNegocioException>(() =>
            _entorno.En<IUsuarioService, EmpleadoCreadoDto>(u => u.CrearEmpleado("Otra Maria", "mlopez@hotel.com", RolEmpleado.Recepcion, admin)));
    }

    [Fact]
    public void CambiarRol_ReemplazaElRolAnterior()
    {
        var admin = _entorno.IdPersona("rsilva");
        var pgomez = _entorno.IdPersona("pgomez");

        var actualizado = _entorno.En<IUsuarioService, EmpleadoDto>(u => u.CambiarRol(pgomez, RolEmpleado.Housekeeping, admin));

        Assert.Equal(RolEmpleado.Housekeeping, actualizado.Rol);
        var sesion = _entorno.En<IAuthService, UsuarioAutenticadoDto>(a => a.IniciarSesion("pgomez", "serv2026"));
        Assert.Equal(RolEmpleado.Housekeeping, sesion.RolPersonal);
    }

    [Fact]
    public void ReactivarCuenta_ConservaElRol()
    {
        var admin = _entorno.IdPersona("rsilva");
        var jherrera = _entorno.IdPersona("jherrera");

        var reactivado = _entorno.En<IUsuarioService, EmpleadoDto>(u => u.CambiarEstado(jherrera, true, admin));

        Assert.True(reactivado.Activo);
        Assert.Equal(RolEmpleado.Housekeeping, reactivado.Rol);
        _entorno.En<IAuthService, UsuarioAutenticadoDto>(a => a.IniciarSesion("jherrera", "hk2026"));
    }

    [Fact]
    public void RegistrarHuesped_MenorDeEdad_SeRechaza()
    {
        var nacimiento = _entorno.Hoy.AddYears(-17);
        var error = Assert.Throws<ReglaNegocioException>(() => _entorno.En<IUsuarioService, HuespedDto>(u =>
            u.RegistrarHuesped(new DatosNuevoUsuario("Menor", "menor@email.com", FechaNacimiento: nacimiento, Contrasena: "secreta1"))));
        Assert.Equal("RN-10", error.Regla);
    }
}
