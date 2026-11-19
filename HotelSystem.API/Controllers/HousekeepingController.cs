using HotelSystem.API.Contratos;
using HotelSystem.API.Seguridad;
using HotelSystem.Application.Housekeeping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.API.Controllers;

[Route("api/housekeeping")]
[Authorize(Roles = RolesApi.Housekeeping)]
public sealed class HousekeepingController(IHabitacionService habitaciones) : ApiControllerBase
{
    /// <summary>Plan de limpieza priorizado (HU-11, RF-10): primero las habitaciones con llegada hoy.</summary>
    [HttpGet("plan-limpieza")]
    public IEnumerable<TareaLimpiezaRespuesta> PlanLimpieza() =>
        habitaciones.PlanLimpieza().Select(TareaLimpiezaRespuesta.Desde);
}
