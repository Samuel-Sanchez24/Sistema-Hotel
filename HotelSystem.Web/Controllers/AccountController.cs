using HotelSystem.Application.Usuarios;
using HotelSystem.Domain.Excepciones;
using HotelSystem.Persistence;
using HotelSystem.Web.DTOs.Account;
using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.Helpers;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

/// <summary>Inicio y cierre de sesion (HU-02).</summary>
public class AccountController(IAuthService auth, IWebHostEnvironment entorno) : HotelControllerBase
{
    private static readonly LoginRoleOptionDto[] Roles =
    [
        new(Role.Huesped, "Huesped", "Reservas, tours, servicios y cuenta", Icons.Home, CredencialesDemo.De(Role.Huesped).Display),
        new(Role.Recepcion, "Recepcion", "Habitaciones, check-in/out, facturacion", Icons.User, CredencialesDemo.De(Role.Recepcion).Display),
        new(Role.Housekeeping, "Housekeeping", "Estado y limpieza de habitaciones", Icons.Rooms, CredencialesDemo.De(Role.Housekeeping).Display),
        new(Role.Coordinador, "Coord. Tours", "Cupos, guias y vehiculos", Icons.Map, CredencialesDemo.De(Role.Coordinador).Display),
        new(Role.Servicios, "Servicios", "Registrar consumos adicionales", Icons.Plus, CredencialesDemo.De(Role.Servicios).Display),
        new(Role.Admin, "Administrador", "Usuarios y reportes de gestion", Icons.Chart, CredencialesDemo.De(Role.Admin).Display),
    ];

    [HttpGet]
    public IActionResult Login(Role? role)
    {
        var current = HttpContext.Session.GetRole();
        if (current is not null && HttpContext.Session.GetUserId() is not null)
        {
            var home = Navigation.Home(current.Value);
            return RedirectToAction(home.Action, home.Controller);
        }

        var page = BuildPage(role, role is null ? "" : CredencialesDemo.De(role.Value).Username);
        page.Info = TempData["Info"] as string;
        return View(page);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Login(LoginDto dto)
    {
        UsuarioAutenticadoDto usuario;
        try
        {
            usuario = auth.IniciarSesion(dto.Username, dto.Password);
        }
        catch (ReglaNegocioException ex)
        {
            return ConError(dto, ex.Message);
        }

        // Cada usuario entra unicamente por el perfil de su rol (RN-08, HU-02 Esc.4).
        if (usuario.PerfilWeb() != dto.Role)
        {
            return ConError(dto, AuthService.MensajeCredencialesInvalidas);
        }

        HttpContext.Session.SetString(SessionKeys.Role, dto.Role.ToString());
        HttpContext.Session.SetString(SessionKeys.UserName, usuario.Nombre);
        HttpContext.Session.SetInt32(SessionKeys.UserId, usuario.Id);

        var destino = Navigation.Home(dto.Role);
        return RedirectToAction(destino.Action, destino.Controller);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction(nameof(Login));
    }

    /// <summary>
    /// Borra la base de datos y la vuelve a crear con los datos de demostracion.
    /// Solo existe en el entorno de desarrollo, para dejar el sistema listo antes de una sustentacion.
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReiniciarDatos()
    {
        if (!entorno.IsDevelopment()) return NotFound();

        HttpContext.RequestServices.ReiniciarBaseDeDatos();
        HttpContext.Session.Clear();
        TempData["Info"] = "Datos de demostracion reiniciados.";
        return RedirectToAction(nameof(Login));
    }

    private IActionResult ConError(LoginDto dto, string mensaje)
    {
        var page = BuildPage(dto.Role, dto.Username);
        page.Error = mensaje;
        return View(nameof(Login), page);
    }

    private LoginPageDto BuildPage(Role? role, string username) => new()
    {
        Roles = Roles,
        SelectedRole = role,
        Username = username,
        DemoPassword = role is null ? "" : CredencialesDemo.De(role.Value).Password,
        CanResetData = entorno.IsDevelopment(),
    };
}
