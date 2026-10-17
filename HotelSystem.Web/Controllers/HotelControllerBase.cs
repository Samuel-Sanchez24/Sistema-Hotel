using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

public abstract class HotelControllerBase(IHotelDataService data) : Controller
{
    protected IHotelDataService Data { get; } = data;

    protected void Toast(string message, ToastType type = ToastType.Success)
    {
        TempData["Toast"] = message;
        TempData["ToastType"] = type.ToString();
    }
}
