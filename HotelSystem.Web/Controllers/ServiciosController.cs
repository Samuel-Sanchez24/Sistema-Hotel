using System.Globalization;
using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.Housekeeping;
using HotelSystem.Application.ServiciosAdicionales;
using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.DTOs.Servicios;
using HotelSystem.Web.Filters;
using HotelSystem.Web.Helpers;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

/// <summary>Personal de servicios: registro de consumos (HU-08).</summary>
[RoleAuthorize(Role.Servicios)]
public class ServiciosController(
    IServicioAdicionalService servicios,
    IHabitacionService habitaciones,
    IReloj reloj) : HotelControllerBase
{
    public IActionResult Index(string? hab = null) => View(BuildRegister(hab));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Registrar(RegisterConsumptionDto dto)
    {
        var model = BuildRegister(dto.RoomNumber);
        model.Service = dto.Service;
        model.Amount = dto.Amount;
        if (!string.IsNullOrWhiteSpace(dto.Date)) model.Date = dto.Date;

        if (string.IsNullOrEmpty(dto.RoomNumber) || string.IsNullOrEmpty(dto.Service) || string.IsNullOrEmpty(dto.Amount))
        {
            model.Error = "Complete todos los campos.";
            return View(nameof(Index), model);
        }

        var digits = new string(dto.Amount.Where(char.IsDigit).ToArray());
        if (digits.Length == 0 || !decimal.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var amount) || amount <= 0)
        {
            model.Error = "Ingrese un monto valido.";
            return View(nameof(Index), model);
        }

        // El backend valida la RN-06 (solo consumos de hoy) y la RN-07 (cuenta abierta),
        // y carga el consumo a la cuenta unica de la estancia.
        var registro = new RegistroConsumo(dto.RoomNumber, null, dto.Service, 1, amount, Ui.ParseDate(dto.Date), UsuarioId);
        model.Error = Intentar(() => servicios.RegistrarConsumo(registro));
        if (model.Error is not null) return View(nameof(Index), model);

        Toast($"Consumo registrado: {dto.Service} ({Ui.Money(amount)}) - Hab. {dto.RoomNumber}");
        return RedirectToAction(nameof(Index));
    }

    public IActionResult Historial() =>
        View(servicios.ListarConsumos().Select(c => c.AWeb()).ToList());

    private RegisterConsumptionPageDto BuildRegister(string? roomNumber)
    {
        var occupied = habitaciones.Listar()
            .Select(h => h.AWeb())
            .Where(r => r.Status == RoomStatus.Ocupada)
            .ToList();

        return new RegisterConsumptionPageDto
        {
            OccupiedRooms = occupied,
            ActiveServices = servicios.ListarCatalogo(soloActivos: true).Select(s => s.AWeb()).ToList(),
            RoomNumber = roomNumber,
            SelectedRoom = occupied.FirstOrDefault(r => r.Number == roomNumber),
            Date = Ui.IsoDate(reloj.Hoy),
        };
    }
}
