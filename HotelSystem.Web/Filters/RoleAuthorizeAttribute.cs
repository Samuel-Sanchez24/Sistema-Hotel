using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HotelSystem.Web.Filters;
public class RoleAuthorizeAttribute(Role role) : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.HttpContext.Session.GetRole() != role)
            context.Result = new RedirectToActionResult("Login", "Account", null);
    }
}
