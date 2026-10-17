using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.DTOs.Recepcion;
using HotelSystem.Web.Filters;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

[RoleAuthorize(Role.Recepcion)]
public class RecepcionController(IHotelDataService data) : HotelControllerBase(data)
{
    /// <summary>Noches facturadas en la prefactura del prototipo.</summary>
    private const int BillingNights = 4;

    /* ─── HU-05: Panel de habitaciones ─── */

    public IActionResult Index(int piso = 1, RoomStatus? estado = null, int? hab = null)
    {
        var rooms = Data.GetRooms();
        return View(new RoomsPanelDto
        {
            Floor = piso,
            Filter = estado,
            FloorRooms = rooms.Where(r => r.Floor == piso && (estado is null || r.Status == estado)).ToList(),
            Counts = Enum.GetValues<RoomStatus>().ToDictionary(s => s, s => rooms.Count(r => r.Status == s)),
            Selected = hab is null ? null : rooms.FirstOrDefault(r => r.Id == hab),
        });
    }

    /* ─── HU-06: Check-in / Check-out ─── */

    public IActionResult CheckInOut(string tab = "checkin", string? q = null)
    {
        var checkIns = Data.GetTodayCheckIns();
        var checkOuts = Data.GetTodayCheckOuts();
        var list = tab == "checkout" ? checkOuts : checkIns;
        if (!string.IsNullOrWhiteSpace(q))
            list = list.Where(i => i.GuestName.Contains(q, StringComparison.OrdinalIgnoreCase) || i.RoomNumber.Contains(q)).ToList();

        return View(new CheckInOutPageDto
        {
            Tab = tab == "checkout" ? "checkout" : "checkin",
            Query = q,
            Items = list,
            CheckInCount = checkIns.Count,
            CheckOutCount = checkOuts.Count,
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CheckIn(string id)
    {
        var item = Data.RegisterCheckIn(id);
        if (item is not null) Toast($"Check-in: {item.GuestName} - Hab. {item.RoomNumber}");
        return RedirectToAction(nameof(CheckInOut), new { tab = "checkin" });
    }

    /* ─── HU-06 / HU-12: Prefactura, pago y cierre ─── */

    public IActionResult Facturacion(string? q = null, int? hab = null, string? numero = null, bool generada = false)
    {
        var occupied = Data.GetRooms().Where(r => r.Status == RoomStatus.Ocupada).ToList();
        var selected = hab is not null
            ? occupied.FirstOrDefault(r => r.Id == hab)
            : numero is not null ? occupied.FirstOrDefault(r => r.Number == numero) : null;

        var model = BuildBilling(selected, generada);
        model.Query = selected is not null ? selected.GuestName : q;
        if (selected is null && !string.IsNullOrWhiteSpace(q))
            model.Results = occupied.Where(r => (r.GuestName ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) || r.Number.Contains(q)).ToList();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GenerarPrefactura(int hab) =>
        RedirectToAction(nameof(Facturacion), new { hab, generada = true });

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CerrarCuenta(BillingPaymentDto dto)
    {
        var room = Data.GetRoom(dto.RoomId);
        if (room is null || room.Status != RoomStatus.Ocupada) return RedirectToAction(nameof(Facturacion));

        var model = BuildBilling(room, generated: true);
        model.Query = room.GuestName;
        model.PayMethod = dto.PayMethod;
        model.PaidAmount = dto.PaidAmount;

        if (string.IsNullOrEmpty(dto.PayMethod))
        {
            model.Error = "Seleccione un metodo de pago para cerrar la cuenta (RN-07 - HU-06).";
            return View(nameof(Facturacion), model);
        }

        var digits = new string((dto.PaidAmount ?? "").Where(char.IsDigit).ToArray());
        var paid = digits.Length == 0 ? 0 : decimal.Parse(digits);
        if (paid != model.GrandTotal)
        {
            model.Error = $"El monto pagado debe coincidir con la prefactura: {Helpers.Ui.Money(model.GrandTotal)} (HU-06 Esc.5).";
            return View(nameof(Facturacion), model);
        }

        // Copia para mostrar el resumen: al cerrar, la habitacion queda libre y en limpieza.
        model.Selected = new RoomDto { Id = room.Id, Number = room.Number, Type = room.Type, GuestName = room.GuestName };
        Data.CloseAccount(room.Id);
        model.Closed = true;
        return View(nameof(Facturacion), model);
    }

    private BillingPageDto BuildBilling(RoomDto? selected, bool generated)
    {
        var consumptions = selected is null ? [] : Data.GetConsumptions().Where(c => c.RoomNumber == selected.Number).ToList();
        return new BillingPageDto
        {
            Selected = selected,
            Consumptions = consumptions,
            Nights = BillingNights,
            RoomTotal = selected is null ? 0 : BillingNights * selected.Price,
            ServicesTotal = consumptions.Sum(c => c.Amount),
            Generated = generated && selected is not null,
            PaymentMethods = MockData.PaymentMethods,
        };
    }
}
