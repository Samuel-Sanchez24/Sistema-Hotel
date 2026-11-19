using System.Globalization;
using HotelSystem.Application.CierreCuenta;
using HotelSystem.Application.Housekeeping;
using HotelSystem.Application.Reservas;
using HotelSystem.Domain.Entidades;
using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.DTOs.Recepcion;
using HotelSystem.Web.Filters;
using HotelSystem.Web.Helpers;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

[RoleAuthorize(Role.Recepcion)]
public class RecepcionController(
    IHabitacionService habitaciones,
    IReservaService reservas,
    ICuentaService cuentas,
    PanelRecepcion panel) : HotelControllerBase
{
    /* ─── HU-05: Panel de habitaciones ─── */

    public IActionResult Index(int piso = 1, RoomStatus? estado = null, int? hab = null)
    {
        var rooms = habitaciones.Listar().Select(h => h.AWeb()).ToList();
        return View(new RoomsPanelDto
        {
            Floor = piso,
            Filter = estado,
            FloorRooms = rooms.Where(r => r.Floor == piso && (estado is null || r.Status == estado)).ToList(),
            Counts = Enum.GetValues<RoomStatus>().ToDictionary(s => s, s => rooms.Count(r => r.Status == s)),
            Selected = hab is null ? null : rooms.FirstOrDefault(r => r.Id == hab.Value),

            // Observer: cambios de estado que las habitaciones notificaron al panel de recepcion.
            News = panel.UltimasNovedades
                .Take(6)
                .Select(n => new RoomNewsDto(n.Numero, n.EstadoAnterior.AWeb(), n.EstadoNuevo.AWeb(), Ui.Stamp(n.Momento)))
                .ToList(),
        });
    }

    /* ─── HU-06: Check-in / Check-out ─── */

    public IActionResult CheckInOut(string tab = "checkin", string? q = null)
    {
        var checkIns = reservas.LlegadasDeHoy().Select(m => m.AWeb()).ToList();
        var checkOuts = reservas.SalidasDeHoy().Select(m => m.AWeb()).ToList();
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
        if (ReservaHabitacion.TryIdDesdeCodigo(id, out var reservaId))
        {
            // El backend exige que la habitacion este Disponible (RN-02) y la deja Ocupada.
            MovimientoDto? movimiento = null;
            var error = Intentar(() => movimiento = reservas.RegistrarCheckIn(reservaId, UsuarioId));

            if (error is not null || movimiento is null) Toast(error ?? "No fue posible registrar el check-in.", ToastType.Error);
            else Toast($"Check-in: {movimiento.Huesped} - Hab. {movimiento.NumeroHabitacion}");
        }

        return RedirectToAction(nameof(CheckInOut), new { tab = "checkin" });
    }

    /* ─── HU-06 / HU-12: Prefactura, pago y cierre ─── */

    public IActionResult Facturacion(string? q = null, int? hab = null, string? numero = null, bool generada = false)
    {
        var occupied = OccupiedRooms();
        var selected = hab is not null
            ? occupied.FirstOrDefault(r => r.Id == hab.Value)
            : numero is not null ? occupied.FirstOrDefault(r => r.Number == numero) : null;

        var model = BuildBilling(selected, generada);
        model.Query = selected is not null ? selected.GuestName : q;
        if (selected is null && !string.IsNullOrWhiteSpace(q))
            model.Results = occupied.Where(r => (r.GuestName ?? "").Contains(q, StringComparison.OrdinalIgnoreCase) || r.Number.Contains(q)).ToList();
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult GenerarPrefactura(int hab)
    {
        // Fachada: una sola llamada consolida alojamiento, servicios y tours, y guarda la prefactura.
        var error = Intentar(() => cuentas.GenerarPrefactura(hab, UsuarioId));
        if (error is not null)
        {
            Toast(error, ToastType.Error);
            return RedirectToAction(nameof(Facturacion));
        }

        return RedirectToAction(nameof(Facturacion), new { hab, generada = true });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CerrarCuenta(BillingPaymentDto dto)
    {
        var room = OccupiedRooms().FirstOrDefault(r => r.Id == dto.RoomId);
        if (room is null) return RedirectToAction(nameof(Facturacion));

        var model = BuildBilling(room, generated: true);
        model.Query = room.GuestName;
        model.PayMethod = dto.PayMethod;
        model.PaidAmount = dto.PaidAmount;

        var digits = new string((dto.PaidAmount ?? "").Where(char.IsDigit).ToArray());
        var paid = decimal.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out var monto) ? monto : 0m;
        var (medioPago, detallePago) = MediosPago.DeCobro(dto.PayMethod);
        var solicitud = new SolicitudCierreCuenta(room.Id, paid, medioPago, detallePago, UsuarioId);

        // Fachada: valida el pago contra el monto consolidado, cierra la cuenta (RN-07),
        // finaliza la reserva y deja la habitacion en limpieza (RN-02). Todo o nada.
        PrefacturaDto? cierre = null;
        model.Error = Intentar(() => cierre = cuentas.CerrarCuenta(solicitud));
        if (cierre is null) return View(nameof(Facturacion), model);

        model.Closed = true;
        model.PrefacturaId = cierre.IdPrefactura;
        return View(nameof(Facturacion), model);
    }

    private List<RoomDto> OccupiedRooms() =>
        habitaciones.Listar()
            .Select(h => h.AWeb())
            .Where(r => r.Status == RoomStatus.Ocupada)
            .ToList();

    private BillingPageDto BuildBilling(RoomDto? selected, bool generated)
    {
        // Cuenta consolidada de la estancia en curso: noches segun las fechas de la reserva,
        // consumos de servicios adicionales y tours (RN-07).
        var cuenta = selected is null ? null : cuentas.ConsultarPorHabitacion(selected.Id);
        if (cuenta is null)
        {
            return new BillingPageDto { PaymentMethods = MediosPago.DeRecepcion };
        }

        return new BillingPageDto
        {
            Selected = selected,
            Consumptions = cuenta.Consumos.Select(c => c.AWeb()).ToList(),
            Tours = cuenta.Tours.Select(t => t.AWeb()).ToList(),
            Nights = cuenta.Noches,
            RoomTotal = cuenta.ValorTotalAlojamiento,
            ServicesTotal = cuenta.TotalServicios,
            ToursTotal = cuenta.TotalTours,
            Generated = generated,
            PaymentMethods = MediosPago.DeRecepcion,
        };
    }
}
