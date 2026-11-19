using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HotelSystem.Web.Filters;

/// <summary>
/// Solo deja pasar a quien inicio sesion con el rol indicado (HU-02 Esc.4).
/// Sin sesion, o con otro rol, se redirige al login.
/// </summary>
public class RoleAuthorizeAttribute(Role role) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var session = context.HttpContext.Session;
        if (session.GetRole() != role || session.GetUserId() is null)
            context.Result = new RedirectToActionResult("Login", "Account", null);
    }
}
