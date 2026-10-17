using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.DTOs.Housekeeping;
using HotelSystem.Web.Filters;
using HotelSystem.Web.Helpers;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

[RoleAuthorize(Role.Housekeeping)]
public class HousekeepingController(IHotelDataService data) : HotelControllerBase(data)
{
    /* ─── HU-11: Estado de habitaciones e incidencias ─── */

    public IActionResult Index(string filtro = "all", int? incidencia = null)
    {
        var model = BuildPanel(filtro);
        model.IncidentRoom = incidencia is null ? null : Data.GetRoom(incidencia.Value);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ActualizarEstado(UpdateRoomStatusDto dto)
    {
        var room = Data.GetRoom(dto.RoomId);
        if (room is not null)
        {
            Data.SetRoomStatus(dto.RoomId, dto.NewStatus);
            Toast($"Hab. {room.Number} - {Ui.Sc(dto.NewStatus).Label}");
        }
        return RedirectToAction(nameof(Index), new { filtro = dto.Filter });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReportarIncidencia(ReportIncidentDto dto)
    {
        var room = Data.GetRoom(dto.RoomId);
        if (room is null) return RedirectToAction(nameof(Index), new { filtro = dto.Filter });

        if (string.IsNullOrWhiteSpace(dto.Note))
        {
            var model = BuildPanel(dto.Filter);
            model.IncidentRoom = room;
            model.IncidentError = "Debe ingresar una descripcion de la incidencia (HU-11 Esc.3).";
            return View(nameof(Index), model);
        }

        Data.SetRoomStatus(dto.RoomId, RoomStatus.Mantenimiento, dto.Note.Trim());
        Toast($"Hab. {room.Number} - {Ui.Sc(RoomStatus.Mantenimiento).Label}");
        return RedirectToAction(nameof(Index), new { filtro = dto.Filter });
    }

    /* ─── HU-11 / RF-10: Plan de limpieza priorizado ─── */

    public IActionResult PlanLimpieza()
    {
        var pending = Data.GetRooms().Where(r => r.Status == RoomStatus.EnLimpieza).ToList();
        var tasks = pending
            .Select(r => new CleaningTaskDto(r, MockData.ArrivalsToday.Contains(r.Number)))
            .OrderBy(t => t.IsUrgent ? 0 : 1)
            .ToList();

        return View(new CleaningPlanDto
        {
            Tasks = tasks,
            ArrivalsTodayCount = tasks.Count(t => t.IsUrgent),
            PendingCount = pending.Count,
        });
    }

    private HousekeepingPanelDto BuildPanel(string filter)
    {
        var rooms = Data.GetRooms();
        var shown = filter switch
        {
            "en_limpieza" => rooms.Where(r => r.Status == RoomStatus.EnLimpieza),
            "mantenimiento" => rooms.Where(r => r.Status == RoomStatus.Mantenimiento),
            _ => rooms.Where(r => r.Status is RoomStatus.EnLimpieza or RoomStatus.Mantenimiento),
        };

        return new HousekeepingPanelDto
        {
            Filter = filter is "en_limpieza" or "mantenimiento" ? filter : "all",
            Rooms = shown.ToList(),
            CleaningCount = rooms.Count(r => r.Status == RoomStatus.EnLimpieza),
            MaintenanceCount = rooms.Count(r => r.Status == RoomStatus.Mantenimiento),
        };
    }
}
