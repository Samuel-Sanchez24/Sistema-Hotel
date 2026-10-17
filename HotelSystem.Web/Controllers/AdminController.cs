using HotelSystem.Web.DTOs.Admin;
using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.Filters;
using HotelSystem.Web.Helpers;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

[RoleAuthorize(Role.Admin)]
public class AdminController(IHotelDataService data) : HotelControllerBase(data)
{
    private static readonly Role[] AssignableRoles =
        [Role.Recepcion, Role.Housekeeping, Role.Coordinador, Role.Servicios, Role.Admin];

    private static readonly string[] ReportedServices =
        ["Restaurante", "Spa", "Bar", "Lavanderia", "Transporte Aeropuerto"];

    /* ─── HU-01: Gestion de usuarios ─── */

    public IActionResult Index(bool nuevo = false, int? editar = null) => View(new AdminUsersPageDto
    {
        Staff = Data.GetStaff(),
        ShowForm = nuevo,
        EditId = editar,
        AssignableRoles = AssignableRoles,
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(CreateStaffUserDto form)
    {
        if (string.IsNullOrWhiteSpace(form.Name) || string.IsNullOrWhiteSpace(form.Email))
        {
            return View(nameof(Index), new AdminUsersPageDto
            {
                Staff = Data.GetStaff(),
                ShowForm = true,
                Form = form,
                FormError = "Complete nombre y correo.",
                AssignableRoles = AssignableRoles,
            });
        }

        Data.CreateStaff(form.Name.Trim(), form.Email.Trim(), form.Role);
        Toast($"Usuario creado: {form.Name.Trim()} ({Ui.RoleLabel(form.Role)})");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleActivo(int id)
    {
        var u = Data.ToggleStaffActive(id);
        if (u is not null)
            Toast(u.Active
                ? $"Cuenta reactivada: {u.Name} - rol {Ui.RoleLabel(u.Role)} restaurado (HU-01 Esc.5)"
                : $"Cuenta desactivada: {u.Name} (HU-01 Esc.3)");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarRol(ChangeRoleDto dto)
    {
        Data.UpdateStaffRole(dto.UserId, dto.Role);
        Toast("Rol actualizado. Se retiraron los permisos anteriores (RN-08 - HU-01 Esc.4).");
        return RedirectToAction(nameof(Index));
    }

    /* ─── HU-13: Reportes de gestion ─── */

    public IActionResult Reportes(ReportFilterDto filtro)
    {
        var rooms = Data.GetRooms();
        var tours = Data.GetTours();
        var consumptions = Data.GetConsumptions();
        var occupied = rooms.Where(r => r.Status == RoomStatus.Ocupada).ToList();

        return View(new ReportsPageDto
        {
            Filter = filtro,
            TotalRooms = rooms.Count,
            OccupiedCount = occupied.Count,
            AvailableCount = rooms.Count(r => r.Status == RoomStatus.Disponible),
            Occupancy =
            [
                new("Simple", 20, occupied.Count(r => r.Type == RoomType.Simple)),
                new("Doble", 25, occupied.Count(r => r.Type == RoomType.Doble)),
                new("Suite", 15, occupied.Count(r => r.Type == RoomType.Suite)),
            ],
            Income =
            [
                new("Alojamiento", occupied.Sum(r => r.Price * 3)),
                new("Tours", tours.Sum(t => t.Booked * t.Price)),
                new("Servicios", consumptions.Sum(c => c.Amount)),
            ],
            Services = ReportedServices
                .Select(s => new ServiceReportRowDto(s, consumptions.Count(c => c.Service == s), consumptions.Where(c => c.Service == s).Sum(c => c.Amount)))
                .ToList(),
            StaffRows = Data.GetStaff()
                .Where(u => u.Role != Role.Admin)
                .Select((u, i) => new StaffReportRowDto(u.Name, u.Role, u.Active, u.Active ? 12 + i * 4 : 0))
                .ToList(),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Exportar(ReportFilterDto filtro)
    {
        var format = filtro.Format == "Excel" ? "Excel" : "PDF";
        Toast($"{format} generado: Reporte_{filtro.Tab}_{filtro.DateFrom.Replace(' ', '_')}.{format.ToLowerInvariant()} (HU-13 Esc.1)");
        return RedirectToAction(nameof(Reportes), new { filtro.Tab, filtro.DateFrom, filtro.DateTo, filtro.NoData });
    }
}
