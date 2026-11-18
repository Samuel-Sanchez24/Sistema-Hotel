using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.Comun;
using HotelSystem.Domain.Entidades;
using HotelSystem.Domain.Enumeraciones;
using HotelSystem.Domain.Excepciones;

namespace HotelSystem.Application.Usuarios;

public sealed class UsuarioService(
    IPersonaRepository personas,
    CreadorEmpleado creadorEmpleado,
    CreadorHuesped creadorHuesped,
    IUnidadDeTrabajo unidadDeTrabajo,
    Bitacora bitacora) : IUsuarioService
{
    public IReadOnlyList<EmpleadoDto> ListarPersonal() =>
        personas.ListarEmpleados().Select(EmpleadoDto.Desde).ToList();

    public EmpleadoCreadoDto CrearEmpleado(string nombre, string correo, RolEmpleado rol, int administradorId)
    {
        // El servicio usa el creador a través de su tipo abstracto: no sabe qué clase concreta se instancia.
        CreadorUsuario creador = creadorEmpleado;
        var creado = creador.Registrar(new DatosNuevoUsuario(nombre, correo, Rol: rol, RegistradoPorId: administradorId));
        unidadDeTrabajo.GuardarCambios();

        return new EmpleadoCreadoDto(EmpleadoDto.Desde((Empleado)creado.Persona), creado.Contrasena);
    }

    public HuespedDto RegistrarHuesped(DatosNuevoUsuario datos)
    {
        CreadorUsuario creador = creadorHuesped;
        var creado = creador.Registrar(datos);
        unidadDeTrabajo.GuardarCambios();

        return HuespedDto.Desde((Huesped)creado.Persona);
    }

    public EmpleadoDto CambiarEstado(int empleadoId, bool activo, int administradorId)
    {
        var empleado = Buscar(empleadoId);
        if (!activo && empleado.Id == administradorId)
            throw new ReglaNegocioException("No puede desactivar su propia cuenta.");

        if (activo) empleado.Reactivar();
        else empleado.Desactivar();

        bitacora.Registrar(administradorId, TipoOperacion.GestionUsuarios,
            $"Cuenta {(activo ? "reactivada" : "desactivada")}: {empleado.Nombre}");
        unidadDeTrabajo.GuardarCambios();

        return EmpleadoDto.Desde(empleado);
    }

    public EmpleadoDto AlternarEstado(int empleadoId, int administradorId) =>
        CambiarEstado(empleadoId, !Buscar(empleadoId).Activo, administradorId);

    public EmpleadoDto CambiarRol(int empleadoId, RolEmpleado rol, int administradorId)
    {
        var empleado = Buscar(empleadoId);
        if (empleado.Id == administradorId && rol != RolEmpleado.Administrador)
            throw new ReglaNegocioException("No puede quitarse a si mismo el rol de administrador.");

        var anterior = empleado.Rol;
        empleado.CambiarRol(rol);

        bitacora.Registrar(administradorId, TipoOperacion.GestionUsuarios,
            $"Rol de {empleado.Nombre}: {anterior} -> {rol}");
        unidadDeTrabajo.GuardarCambios();

        return EmpleadoDto.Desde(empleado);
    }

    private Empleado Buscar(int empleadoId) =>
        personas.ObtenerEmpleado(empleadoId) ?? throw new EntidadNoEncontradaException("El usuario no existe.");
}
