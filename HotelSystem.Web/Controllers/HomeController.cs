using System.Diagnostics;
using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var role = HttpContext.Session.GetRole();
        if (role is null) return RedirectToAction("Login", "Account");
        var home = Navigation.Home(role.Value);
        return RedirectToAction(home.Action, home.Controller);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() =>
        View(new ErrorDto { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
