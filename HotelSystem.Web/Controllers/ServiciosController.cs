using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.DTOs.Servicios;
using HotelSystem.Web.Filters;
using HotelSystem.Web.Helpers;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

/// <summary>Personal de servicios: registro de consumos (HU-08).</summary>
[RoleAuthorize(Role.Servicios)]
public class ServiciosController(IHotelDataService data) : HotelControllerBase(data)
{
    public IActionResult Index(string? hab = null) => View(BuildRegister(hab));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Registrar(RegisterConsumptionDto dto)
    {
        var model = BuildRegister(dto.RoomNumber);
        model.Service = dto.Service;
        model.Amount = dto.Amount;

        if (string.IsNullOrEmpty(dto.RoomNumber) || string.IsNullOrEmpty(dto.Service) || string.IsNullOrEmpty(dto.Amount))
        {
            model.Error = "Complete todos los campos.";
            return View(nameof(Index), model);
        }

        var digits = new string(dto.Amount.Where(char.IsDigit).ToArray());
        if (digits.Length == 0 || !decimal.TryParse(digits, out var amount) || amount <= 0)
        {
            model.Error = "Ingrese un monto valido.";
            return View(nameof(Index), model);
        }

        Data.AddConsumption(dto.RoomNumber, dto.Service, amount);
        Toast($"Consumo registrado: {dto.Service} ({Ui.Money(amount)}) - Hab. {dto.RoomNumber}");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Historial() => View(Data.GetConsumptions());

    private RegisterConsumptionPageDto BuildRegister(string? roomNumber)
    {
        var occupied = Data.GetRooms().Where(r => r.Status == RoomStatus.Ocupada).ToList();
        return new RegisterConsumptionPageDto
        {
            OccupiedRooms = occupied,
            ActiveServices = Data.GetCatalog().Where(s => s.Active).ToList(),
            RoomNumber = roomNumber,
            SelectedRoom = occupied.FirstOrDefault(r => r.Number == roomNumber),
        };
    }
}
