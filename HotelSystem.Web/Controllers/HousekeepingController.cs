using HotelSystem.Application.Housekeeping;
using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.DTOs.Housekeeping;
using HotelSystem.Web.Filters;
using HotelSystem.Web.Helpers;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

[RoleAuthorize(Role.Housekeeping)]
public class HousekeepingController(IHabitacionService habitaciones) : HotelControllerBase
{
    /* ─── HU-11: Estado de habitaciones e incidencias ─── */

    public IActionResult Index(string filtro = "all", int? incidencia = null)
    {
        var model = BuildPanel(filtro);
        model.IncidentRoom = incidencia is null ? null : habitaciones.Obtener(incidencia.Value)?.AWeb();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ActualizarEstado(UpdateRoomStatusDto dto)
    {
        // El backend aplica el patron State: una transicion no permitida (RN-02) vuelve como mensaje.
        HabitacionDto? actualizada = null;
        var error = Intentar(() => actualizada = habitaciones.CambiarEstado(dto.RoomId, dto.NewStatus.ADominio(), null, UsuarioId));

        if (error is not null || actualizada is null) Toast(error ?? "No fue posible actualizar la habitacion.", ToastType.Error);
        else Toast($"Hab. {actualizada.Numero} - {Ui.Sc(dto.NewStatus).Label}");

        return RedirectToAction(nameof(Index), new { filtro = dto.Filter });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ReportarIncidencia(ReportIncidentDto dto)
    {
        var room = habitaciones.Obtener(dto.RoomId)?.AWeb();
        if (room is null) return RedirectToAction(nameof(Index), new { filtro = dto.Filter });

        // La descripcion es obligatoria (HU-11 Esc.3): el backend rechaza la incidencia sin nota
        // y tambien las transiciones que la RN-02 no permite.
        var nota = dto.Note?.Trim();
        var error = Intentar(() => habitaciones.CambiarEstado(dto.RoomId, RoomStatus.Mantenimiento.ADominio(), nota, UsuarioId));
        if (error is not null)
        {
            var model = BuildPanel(dto.Filter);
            model.IncidentRoom = room;
            model.IncidentNote = dto.Note;
            model.IncidentError = error;
            return View(nameof(Index), model);
        }

        Toast($"Hab. {room.Number} - {Ui.Sc(RoomStatus.Mantenimiento).Label}");
        return RedirectToAction(nameof(Index), new { filtro = dto.Filter });
    }

    /* ─── HU-11 / RF-10: Plan de limpieza priorizado ─── */

    public IActionResult PlanLimpieza()
    {
        // El backend ordena: primero las habitaciones con llegada hoy, luego las que llevan mas tiempo esperando.
        var tasks = habitaciones.PlanLimpieza()
            .Select(t => new CleaningTaskDto(t.Habitacion.AWeb(), t.LlegadaHoy))
            .ToList();

        return View(new CleaningPlanDto
        {
            Tasks = tasks,
            ArrivalsTodayCount = tasks.Count(t => t.IsUrgent),
            PendingCount = tasks.Count,
        });
    }

    private HousekeepingPanelDto BuildPanel(string filter)
    {
        var rooms = habitaciones.Listar().Select(h => h.AWeb()).ToList();
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
