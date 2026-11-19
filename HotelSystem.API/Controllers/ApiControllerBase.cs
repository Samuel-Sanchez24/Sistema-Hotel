using HotelSystem.API.Seguridad;
using HotelSystem.Application.Housekeeping;
using HotelSystem.Domain.Excepciones;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.API.Controllers;

/// <summary>
/// Base de los controladores REST. Igual que en la web MVC, los controladores no tienen
/// reglas de negocio: leen la peticion, llaman al servicio de aplicacion y devuelven el
/// resultado. Las reglas incumplidas se convierten en 4xx en <c>FiltroErroresNegocio</c>.
/// Todas las rutas exigen token, salvo las marcadas con [AllowAnonymous].
/// </summary>
[ApiController]
[Authorize]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Identificador del usuario del token (huesped o empleado).</summary>
    protected int UsuarioId => int.TryParse(User.FindFirst(ClaimsToken.Id)?.Value, out var id) ? id : 0;

    protected static T Requerido<T>(T? valor, string mensaje) where T : struct =>
        valor ?? throw new ReglaNegocioException(mensaje);

    protected static string TextoRequerido(string? valor, string mensaje) =>
        string.IsNullOrWhiteSpace(valor) ? throw new ReglaNegocioException(mensaje) : valor.Trim();

    /// <summary>Busca el identificador de una habitacion a partir de su numero ("305").</summary>
    protected static int IdHabitacion(IHabitacionService habitaciones, string? numero)
    {
        var texto = TextoRequerido(numero, "Indique el numero de la habitacion.");
        return habitaciones.Listar().FirstOrDefault(h => h.Numero == texto)?.Id
            ?? throw new EntidadNoEncontradaException($"La habitacion {texto} no existe.");
    }
}
