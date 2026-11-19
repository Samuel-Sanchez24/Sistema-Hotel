using System.Globalization;
using HotelSystem.Application.Reservas;
using HotelSystem.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

/// <summary>
/// Paneles en vivo (HU-05, HU-11). Los paneles de recepcion y de housekeeping son los
/// observadores de las habitaciones (patron Observer) y aumentan su numero de version con
/// cada cambio que reciben. Las pantallas consultan aqui esa version cada pocos segundos
/// (wwwroot/js/site.js) y se recargan solas cuando cambia.
/// </summary>
public class EventosController(PanelRecepcion recepcion, PanelHousekeeping housekeeping) : Controller
{
    [HttpGet]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Version(string panel = "recepcion")
    {
        if (HttpContext.Session.GetRole() is null) return Unauthorized();

        var version = panel == "housekeeping" ? housekeeping.Version : recepcion.Version;
        return Content(version.ToString(CultureInfo.InvariantCulture), "text/plain");
    }
}
