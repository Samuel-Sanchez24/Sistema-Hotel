using HotelSystem.Application.Tours;
using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.DTOs.Tours;
using HotelSystem.Web.Filters;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

/// <summary>Coordinador de tours (HU-10).</summary>
[RoleAuthorize(Role.Coordinador)]
public class ToursController(ITourService tours) : HotelControllerBase
{
    /* ─── HU-10: Panel de cupos ─── */

    public IActionResult Index(int? detalle = null)
    {
        var lista = tours.Listar().Select(t => t.AWeb()).ToList();
        return View(new ToursPanelDto
        {
            Tours = lista,
            Selected = detalle is null ? null : lista.FirstOrDefault(t => t.Id == detalle.Value),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult AjustarCupo(int id, int delta)
    {
        // +1 bloquea un cupo y -1 lo libera. El backend no deja superar el cupo maximo (RN-11)
        // ni liberar cupos que pertenecen a reservas de huespedes (RN-05).
        var error = delta < 0
            ? Intentar(() => tours.LiberarCupos(id, -delta, UsuarioId))
            : Intentar(() => tours.BloquearCupos(id, delta, UsuarioId));

        if (error is not null) Toast(error, ToastType.Error);
        else Toast(delta < 0 ? "Cupo liberado por cancelacion (HU-10)" : "Cupo bloqueado (HU-10)");

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

        // El formulario envia los nombres; se buscan sus identificadores.
        var recursos = tours.ListarRecursos();
        var guia = recursos.Guias.FirstOrDefault(g => g.Nombre == dto.Guide);
        var vehiculo = recursos.Vehiculos.FirstOrDefault(v => v.Nombre == dto.Vehicle);

        if (dto.TourId is null || guia is null || vehiculo is null)
        {
            model.Error = "Complete todos los campos.";
            return View(nameof(Asignacion), model);
        }

        // RN-03: el backend rechaza la asignacion si el guia o el vehiculo ya estan en otro
        // tour cuyo horario se cruza.
        var tourId = dto.TourId.Value;
        var guiaId = guia.Id;
        var vehiculoId = vehiculo.Id;
        TourResumenDto? asignado = null;
        model.Error = Intentar(() => asignado = tours.AsignarRecursos(tourId, guiaId, vehiculoId, UsuarioId));
        if (asignado is null) return View(nameof(Asignacion), model);

        Toast($"Asignacion guardada: {guia.Nombre} + {vehiculo.Nombre} - {asignado.Nombre}");
        return RedirectToAction(nameof(Asignacion));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Liberar(int id)
    {
        RecursosLiberadosDto? liberados = null;
        var error = Intentar(() => liberados = tours.LiberarRecursos(id, UsuarioId));

        if (error is not null || liberados is null)
        {
            Toast(error ?? "No fue posible liberar los recursos.", ToastType.Error);
        }
        else
        {
            var guia = liberados.NombreGuia ?? "sin guia";
            var vehiculo = liberados.NombreVehiculo ?? "sin vehiculo";
            Toast($"Recursos liberados: {guia} y {vehiculo} disponibles de nuevo (HU-10 Esc.5)");
        }

        return RedirectToAction(nameof(Asignacion));
    }

    private TourAssignmentPageDto BuildAssignment(int? tourId)
    {
        var lista = tours.Listar().Select(t => t.AWeb()).ToList();
        var tour = tourId is null ? null : lista.FirstOrDefault(t => t.Id == tourId.Value);

        // El backend marca como ocupados los recursos asignados a otro tour que se cruza de horario.
        var recursos = tours.ListarRecursos(tour?.Id);

        return new TourAssignmentPageDto
        {
            Tours = lista,
            Selected = tour,
            Guides = recursos.Guias.Select(g => new ResourceOptionDto(g.Nombre, g.Ocupado)).ToList(),
            Vehicles = recursos.Vehiculos.Select(v => new ResourceOptionDto(v.Nombre, v.Ocupado)).ToList(),
        };
    }
}
