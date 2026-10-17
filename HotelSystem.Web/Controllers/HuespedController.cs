using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.DTOs.Huesped;
using HotelSystem.Web.Filters;
using HotelSystem.Web.Helpers;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

/// <summary>Portal del huesped (HU-03, HU-07, HU-09, HU-12).</summary>
[RoleAuthorize(Role.Huesped)]
public class HuespedController(IHotelDataService data) : HotelControllerBase(data)
{
    private const int BookingNights = 3;
    private const int CurrentStayNights = 4;

    /* ─── HU-03: Mis reservas + cancelar ─── */

    public IActionResult Index(string? cancelar = null)
    {
        var reservations = Data.GetGuestReservations();
        return View(new GuestHomeDto
        {
            GuestName = MockData.GuestName,
            Active = reservations.FirstOrDefault(r => r.Status == ReservationStatus.EnCurso),
            Reservations = reservations,
            CancelTarget = reservations.FirstOrDefault(r => r.Id == cancelar && r.Status != ReservationStatus.Cancelada),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancelar(string id)
    {
        var res = Data.CancelGuestReservation(id);
        if (res is not null)
        {
            // RN-09: cancelacion tardia genera cargo de 1 noche.
            if (res.LateCancel) Toast($"Reserva {res.Id} cancelada. Cargo de 1 noche (RN-09).", ToastType.Error);
            else Toast($"Reserva {res.Id} cancelada sin cargo (RN-09).");
        }
        return RedirectToAction(nameof(Index));
    }

    /* ─── HU-03: Reservar habitacion (3 pasos) ─── */

    public IActionResult ReservarHabitacion(BookRoomDto form, int paso = 1)
    {
        var model = BuildBooking(form, paso);
        // Sin disponibilidad o sin habitacion elegida: se regresa al paso valido anterior.
        if (model.Step >= 2 && model.Available.Count == 0) model.Step = 1;
        if (model.Step == 3 && model.SelectedRoom is null) model.Step = 2;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmarReserva(BookRoomDto form)
    {
        var model = BuildBooking(form, 3);
        if (model.SelectedRoom is null) return RedirectToAction(nameof(ReservarHabitacion));

        if (!form.AgeConfirmed)
            model.Error = "Debe confirmar que es mayor de 18 anos (RN-10 - HU-03 Esc.4).";
        else if (string.IsNullOrEmpty(form.PayMethod))
            model.Error = "Debe registrar un medio de pago para confirmar la reserva (RN-04 - HU-03 Esc.3).";
        else
            model.Done = true;

        return View(nameof(ReservarHabitacion), model);
    }

    private BookRoomPageDto BuildBooking(BookRoomDto form, int step)
    {
        var available = Data.GetRooms().Where(r => r.Type == form.Type && r.Status == RoomStatus.Disponible).ToList();
        return new BookRoomPageDto
        {
            Step = Math.Clamp(step, 1, 3),
            Form = form,
            Available = available,
            SelectedRoom = available.FirstOrDefault(r => r.Id == form.RoomId),
            Nights = BookingNights,
            GuestEmail = MockData.GuestEmail,
        };
    }

    /* ─── HU-09: Reservar tour ─── */

    public IActionResult ReservarTour(int? tour = null) => View(BuildTours(tour));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmarTour(BookTourDto form)
    {
        var model = BuildTours(form.TourId);
        model.Form = form;

        if (!form.AgeConfirmed)
            model.Error = "Debe confirmar que es mayor de 18 anos (RN-10 - HU-09 Esc.4).";
        else if (string.IsNullOrEmpty(form.PayMethod))
            model.Error = "Debe registrar un medio de pago o deposito (RN-04 - HU-09 Esc.3).";
        else
        {
            var result = Data.BookGuestTour(form.TourId, Math.Max(1, form.Persons));
            if (result.Success)
            {
                Toast(result.Message);
                return RedirectToAction(nameof(ReservarTour));
            }
            model.Error = result.Message;
        }

        return View(nameof(ReservarTour), model);
    }

    private GuestToursPageDto BuildTours(int? selectedId)
    {
        var booked = Data.GetGuestBookedTourIds();
        var selected = selectedId is null ? null : Data.GetTour(selectedId.Value);
        return new GuestToursPageDto
        {
            Tours = Data.GetTours(),
            ConfirmedIds = booked,
            Selected = selected is { IsFull: false } && !booked.Contains(selected.Id) ? selected : null,
            Form = new BookTourDto { TourId = selectedId ?? 0 },
        };
    }

    /* ─── HU-07: Solicitar servicio ─── */

    public IActionResult SolicitarServicio(int? servicio = null)
    {
        var active = Data.GetCatalog().Where(s => s.Active).ToList();
        return View(new GuestServiceRequestPageDto
        {
            RoomNumber = MockData.GuestRoom,
            GuestName = MockData.GuestName,
            Categories = active.GroupBy(s => s.Category).ToList(),
            Selected = active.FirstOrDefault(s => s.Id == servicio),
            Requests = Data.GetServiceRequests().Where(r => r.RoomNumber == MockData.GuestRoom).ToList(),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Solicitar(int servicioId)
    {
        var req = Data.AddServiceRequest(servicioId, MockData.GuestRoom);
        if (req is not null) Toast($"Solicitud enviada: {req.ServiceName} - Hab. {req.RoomNumber}");
        return RedirectToAction(nameof(SolicitarServicio));
    }

    /* ─── HU-07: Mis consumos ─── */

    public IActionResult Consumos() => View(new GuestConsumptionsDto
    {
        RoomNumber = MockData.GuestRoom,
        Items = Data.GetConsumptions().Where(c => c.RoomNumber == MockData.GuestRoom).ToList(),
    });

    /* ─── HU-12: Mi cuenta / prefactura ─── */

    public IActionResult MiCuenta(bool factura = false) => View(new GuestBillDto
    {
        GuestName = MockData.GuestName,
        RoomNumber = MockData.GuestRoom,
        Stay = "9-13 Sep 2026",
        Nights = CurrentStayNights,
        PricePerNight = MockData.PriceByType[RoomType.Doble],
        Consumptions = Data.GetConsumptions().Where(c => c.RoomNumber == MockData.GuestRoom).ToList(),
        ShowInvoiceModal = factura,
    });
}
