using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.DTOs.Tours;
using HotelSystem.Web.Filters;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

/// <summary>Coordinador de tours (HU-10).</summary>
[RoleAuthorize(Role.Coordinador)]
public class ToursController(IHotelDataService data) : HotelControllerBase(data)
{
    /* ─── HU-10: Panel de cupos ─── */

    public IActionResult Index(int? detalle = null) => View(new ToursPanelDto
    {
        Tours = Data.GetTours(),
        Selected = detalle is null ? null : Data.GetTour(detalle.Value),
    });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AjustarCupo(int id, int delta)
    {
        Data.AdjustTourQuota(id, delta);
        Toast(delta < 0 ? "Cupo liberado por cancelacion (HU-10)" : "Cupo ajustado");
        return RedirectToAction(nameof(Index));
    }

    /* ─── HU-10 / RN-03: Asignacion de recursos ─── */

    public IActionResult Asignacion(int? tourId = null) => View(BuildAssignment(tourId));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Asignar(AssignResourcesDto dto)
    {
        var model = BuildAssignment(dto.TourId);
        model.Guide = dto.Guide;
        model.Vehicle = dto.Vehicle;

        if (dto.TourId is null || string.IsNullOrEmpty(dto.Guide) || string.IsNullOrEmpty(dto.Vehicle))
        {
            model.Error = "Complete todos los campos.";
            return View(nameof(Asignacion), model);
        }

        var result = Data.AssignTourResources(dto.TourId.Value, dto.Guide, dto.Vehicle);
        if (!result.Success)
        {
            model.Error = result.Message;
            return View(nameof(Asignacion), model);
        }

        Toast(result.Message);
        return RedirectToAction(nameof(Asignacion));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Liberar(int id)
    {
        var released = Data.ReleaseTourResources(id);
        if (released is not null)
            Toast($"Recursos liberados: {released.Guide} y {released.Vehicle} disponibles de nuevo (HU-10 Esc.5)");
        return RedirectToAction(nameof(Asignacion));
    }

    private TourAssignmentPageDto BuildAssignment(int? tourId)
    {
        var tours = Data.GetTours();
        var tour = tours.FirstOrDefault(t => t.Id == tourId);
        var sameSlot = tour is null
            ? []
            : tours.Where(t => t.Id != tour.Id && t.Date == tour.Date && t.Time == tour.Time).ToList();

        return new TourAssignmentPageDto
        {
            Tours = tours,
            Selected = tour,
            Guides = Data.GetGuides().Select(g => new ResourceOptionDto(g, sameSlot.Any(t => t.Guide == g))).ToList(),
            Vehicles = Data.GetVehicles().Select(v => new ResourceOptionDto(v, sameSlot.Any(t => t.Vehicle == v))).ToList(),
        };
    }
}
