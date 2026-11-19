using HotelSystem.Domain.Excepciones;
using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

/// <summary>
/// Base de los controladores. Cada controlador recibe por constructor solo los servicios
/// de aplicacion que necesita (segregacion de interfaces): ya no existe una interfaz unica
/// con todas las operaciones del hotel.
///
/// Los controladores no contienen reglas de negocio: leen el formulario, llaman al servicio
/// y eligen la vista. Si el servicio rechaza la operacion, muestran su mensaje.
/// </summary>
public abstract class HotelControllerBase : Controller
{
    /// <summary>Identificador del usuario que inicio sesion (huesped o empleado).</summary>
    protected int UsuarioId => HttpContext.Session.GetUserId() ?? 0;

    protected void Toast(string message, ToastType type = ToastType.Success)
    {
        TempData["Toast"] = message;
        TempData["ToastType"] = type.ToString();
    }

    /// <summary>
    /// Ejecuta una operacion del backend. Devuelve nulo si salio bien, o el mensaje de la
    /// regla de negocio incumplida para mostrarlo al usuario.
    /// </summary>
    protected static string? Intentar(Action operacion)
    {
        try
        {
            operacion();
            return null;
        }
        catch (ReglaNegocioException ex)
        {
            return ex.Message;
        }
    }
}
