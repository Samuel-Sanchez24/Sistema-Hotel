using HotelSystem.Web.DTOs.Account;
using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.Helpers;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

public class AccountController(IHotelDataService data) : HotelControllerBase(data)
{
    private static readonly LoginRoleOptionDto[] Roles =
    [
        new(Role.Huesped, "Huesped", "Reservas, tours, servicios y cuenta", Icons.Home, MockData.DemoCredentials[Role.Huesped].Display),
        new(Role.Recepcion, "Recepcion", "Habitaciones, check-in/out, facturacion", Icons.User, MockData.DemoCredentials[Role.Recepcion].Display),
        new(Role.Housekeeping, "Housekeeping", "Estado y limpieza de habitaciones", Icons.Rooms, MockData.DemoCredentials[Role.Housekeeping].Display),
        new(Role.Coordinador, "Coord. Tours", "Cupos, guias y vehiculos", Icons.Map, MockData.DemoCredentials[Role.Coordinador].Display),
        new(Role.Servicios, "Servicios", "Registrar consumos adicionales", Icons.Plus, MockData.DemoCredentials[Role.Servicios].Display),
        new(Role.Admin, "Administrador", "Usuarios y reportes de gestion", Icons.Chart, MockData.DemoCredentials[Role.Admin].Display),
    ];

    [HttpGet]
    public IActionResult Login(Role? role)
    {
        var current = HttpContext.Session.GetRole();
        if (current is not null)
        {
            var home = Navigation.Home(current.Value);
            return RedirectToAction(home.Action, home.Controller);
        }

        return View(BuildPage(role, role is null ? "" : MockData.DemoCredentials[role.Value].Username));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginDto dto)
    {
        var result = Data.ValidateLogin(dto);
        if (!result.Success)
        {
            var page = BuildPage(dto.Role, dto.Username);
            page.Error = result.Error;
            return View(page);
        }

        HttpContext.Session.SetString(SessionKeys.Role, dto.Role.ToString());
        HttpContext.Session.SetString(SessionKeys.UserName, result.DisplayName);
        var home = Navigation.Home(dto.Role);
        return RedirectToAction(home.Action, home.Controller);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    private static LoginPageDto BuildPage(Role? role, string username) => new()
    {
        Roles = Roles,
        SelectedRole = role,
        Username = username,
        DemoPassword = role is null ? "" : MockData.DemoCredentials[role.Value].Password,
    };
}
