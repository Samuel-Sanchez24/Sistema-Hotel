using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.CierreCuenta;
using HotelSystem.Application.Housekeeping;
using HotelSystem.Application.Reservas;
using HotelSystem.Application.ServiciosAdicionales;
using HotelSystem.Application.Tours;
using HotelSystem.Domain.Entidades;
using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.DTOs.Huesped;
using HotelSystem.Web.Filters;
using HotelSystem.Web.Helpers;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

/// <summary>Portal del huesped (HU-03, HU-04, HU-07, HU-09, HU-12).</summary>
[RoleAuthorize(Role.Huesped)]
public class HuespedController(
    IReservaService reservas,
    ITourService tours,
    IServicioAdicionalService servicios,
    ICuentaService cuentas,
    IReloj reloj) : HotelControllerBase
{
    private string NombreHuesped => HttpContext.Session.GetUserName();

    /* ─── HU-03: Mis reservas + cancelar ─── */

    public IActionResult Index(string? cancelar = null)
    {
        var lista = reservas.ReservasDeHuesped(UsuarioId).Select(r => r.AWeb()).ToList();
        return View(new GuestHomeDto
        {
            GuestName = NombreHuesped,
            Active = lista.FirstOrDefault(r => r.Status == ReservationStatus.EnCurso),
            Reservations = lista,
            CancelTarget = lista.FirstOrDefault(r => r.Id == cancelar && r.Status != ReservationStatus.Cancelada),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Cancelar(string id)
    {
        if (ReservaTour.TryIdDesdeCodigo(id, out var reservaTourId))
        {
            var errorTour = Intentar(() => tours.CancelarReserva(reservaTourId, UsuarioId));
            if (errorTour is null) Toast($"Reserva {id} cancelada. Los cupos del tour quedaron libres.");
            else Toast(errorTour, ToastType.Error);

            return RedirectToAction(nameof(Index));
        }

        if (!ReservaHabitacion.TryIdDesdeCodigo(id, out var reservaId))
        {
            return RedirectToAction(nameof(Index));
        }

        // RN-09: el backend decide si la cancelacion es tardia y cuanto se cobra.
        CancelacionDto? resultado = null;
        var error = Intentar(() => resultado = reservas.CancelarReserva(reservaId, UsuarioId));

        if (error is not null || resultado is null)
        {
            Toast(error ?? "No fue posible cancelar la reserva.", ToastType.Error);
        }
        else if (resultado.Tardia)
        {
            Toast($"Reserva {resultado.Codigo} cancelada. Cargo de 1 noche: {Ui.Money(resultado.Cargo)} (RN-09).", ToastType.Error);
        }
        else
        {
            Toast($"Reserva {resultado.Codigo} cancelada sin cargo (RN-09).");
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
        var room = model.SelectedRoom;
        if (room is null) return RedirectToAction(nameof(ReservarHabitacion));

        // El backend valida RN-10 (mayoria de edad), RN-04 (medio de pago), RN-11 (capacidad)
        // y RN-01 (disponibilidad); si todo pasa, guarda la reserva y envia el correo (HU-04).
        var (medioPago, detallePago) = MediosPago.DeReserva(form.PayMethod);
        var solicitud = new SolicitudReservaHabitacion(
            UsuarioId, room.Id, model.CheckInDate, model.CheckOutDate, form.Guests, form.AgeConfirmed, medioPago, detallePago);

        ReservaConfirmadaDto? confirmada = null;
        model.Error = Intentar(() => confirmada = reservas.ReservarHabitacion(solicitud));

        if (confirmada is not null)
        {
            model.Done = true;
            model.ReservationCode = confirmada.Codigo;
            model.GuestEmail = confirmada.CorreoDestino;
            model.EmailSent = confirmada.CorreoEnviado;
        }

        return View(nameof(ReservarHabitacion), model);
    }

    private BookRoomPageDto BuildBooking(BookRoomDto form, int step)
    {
        // Fechas del formulario (campos de fecha HTML). Por defecto: dentro de una semana, tres noches.
        var hoy = reloj.Hoy;
        var entrada = Ui.ParseDate(form.CheckIn) ?? hoy.AddDays(7);
        var salida = Ui.ParseDate(form.CheckOut) ?? entrada.AddDays(3);
        form.CheckIn = Ui.IsoDate(entrada);
        form.CheckOut = Ui.IsoDate(salida);

        var tipos = reservas.TiposDeHabitacion();
        var model = new BookRoomPageDto
        {
            Step = Math.Clamp(step, 1, 3),
            Form = form,
            CheckInDate = entrada,
            CheckOutDate = salida,
            CheckInLabel = Ui.LongDate(entrada),
            CheckOutLabel = Ui.LongDate(salida),
            MinDate = Ui.IsoDate(hoy),
            Nights = Math.Max(0, salida.DayNumber - entrada.DayNumber),
            PriceByType = tipos.ToDictionary(t => t.Tipo.AWeb(), t => t.PrecioNoche),
            CapacityByType = tipos.ToDictionary(t => t.Tipo.AWeb(), t => t.Capacidad),
        };

        // RN-01: solo se ofrecen habitaciones libres en todo el rango de fechas.
        IReadOnlyList<HabitacionDto> disponibles = [];
        var tipo = form.Type.ADominio();
        var huespedes = Math.Max(1, form.Guests);
        model.DateError = Intentar(() => disponibles = reservas.BuscarDisponibles(tipo, entrada, salida, huespedes));

        model.Available = disponibles.Select(h => h.AWeb()).ToList();
        model.SelectedRoom = model.Available.FirstOrDefault(r => r.Id == form.RoomId);
        return model;
    }

    /* ─── HU-09: Reservar tour ─── */

    public IActionResult ReservarTour(int? tour = null) => View(BuildTours(tour));

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ConfirmarTour(BookTourDto form)
    {
        // El backend valida RN-10, RN-04 y RN-11, bloquea los cupos (RN-05) y carga el tour a la cuenta (RN-07).
        var (medioPago, detallePago) = MediosPago.DeReserva(form.PayMethod);
        var solicitud = new SolicitudReservaTour(UsuarioId, form.TourId, Math.Max(1, form.Persons), form.AgeConfirmed, medioPago, detallePago);

        ReservaTourDto? reserva = null;
        var error = Intentar(() => reserva = tours.ReservarTour(solicitud));

        if (error is null && reserva is not null)
        {
            Toast($"Tour reservado: {reserva.NombreTour} - {reserva.NumeroPersonas} persona(s)");
            return RedirectToAction(nameof(ReservarTour));
        }

        var model = BuildTours(form.TourId);
        model.Form = form;
        model.Error = error;

        // Si el formulario ya no se puede mostrar (por ejemplo, el tour se lleno), el mensaje va como aviso.
        if (model.Selected is null)
        {
            Toast(error ?? "No fue posible reservar el tour.", ToastType.Error);
            return RedirectToAction(nameof(ReservarTour));
        }

        return View(nameof(ReservarTour), model);
    }

    private GuestToursPageDto BuildTours(int? selectedId)
    {
        // Se ofrecen los tours de hoy en adelante.
        var hoy = reloj.Hoy;
        var lista = tours.Listar().Where(t => t.Fecha >= hoy).Select(t => t.AWeb()).ToList();
        var reservados = tours.ReservasDeHuesped(UsuarioId)
            .Where(r => r.EstaVigente)
            .Select(r => r.TourId)
            .ToHashSet();
        var selected = selectedId is null ? null : lista.FirstOrDefault(t => t.Id == selectedId.Value);

        return new GuestToursPageDto
        {
            Tours = lista,
            ConfirmedIds = reservados,
            Selected = selected is { IsFull: false } && !reservados.Contains(selected.Id) ? selected : null,
            Form = new BookTourDto { TourId = selectedId ?? 0 },
        };
    }

    /* ─── HU-07: Solicitar servicio ─── */

    public IActionResult SolicitarServicio(int? servicio = null)
    {
        var activos = servicios.ListarCatalogo(soloActivos: true).Select(s => s.AWeb()).ToList();
        var cuenta = cuentas.ConsultarDeHuesped(UsuarioId);

        return View(new GuestServiceRequestPageDto
        {
            RoomNumber = cuenta?.NumeroHabitacion ?? "-",
            GuestName = NombreHuesped,
            Categories = activos.GroupBy(s => s.Category).ToList(),
            Selected = servicio is null ? null : activos.FirstOrDefault(s => s.Id == servicio.Value),
            Requests = servicios.SolicitudesDeHuesped(UsuarioId).Select(s => s.AWeb()).ToList(),
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Solicitar(int servicioId)
    {
        // La solicitud queda asociada a la habitacion de la estancia en curso (RF-05).
        SolicitudServicioDto? solicitud = null;
        var error = Intentar(() => solicitud = servicios.SolicitarServicio(UsuarioId, servicioId));

        if (error is not null || solicitud is null) Toast(error ?? "No fue posible registrar la solicitud.", ToastType.Error);
        else Toast($"Solicitud enviada: {solicitud.NombreServicio} - Hab. {solicitud.NumeroHabitacion}");

        return RedirectToAction(nameof(SolicitarServicio));
    }

    /* ─── HU-07: Mis consumos ─── */

    public IActionResult Consumos()
    {
        var cuenta = cuentas.ConsultarDeHuesped(UsuarioId);
        return View(new GuestConsumptionsDto
        {
            RoomNumber = cuenta?.NumeroHabitacion ?? "-",
            Items = servicios.ConsumosDeHuesped(UsuarioId).Select(c => c.AWeb()).ToList(),
        });
    }

    /* ─── HU-12: Mi cuenta / prefactura ─── */

    public IActionResult MiCuenta(bool factura = false)
    {
        // Cuenta consolidada de la estancia en curso: alojamiento + servicios + tours (RN-07).
        var cuenta = cuentas.ConsultarDeHuesped(UsuarioId);
        if (cuenta is null)
        {
            return View(new GuestBillDto { HasStay = false, GuestName = NombreHuesped, RoomNumber = "-" });
        }

        return View(new GuestBillDto
        {
            GuestName = cuenta.NombreHuesped,
            RoomNumber = cuenta.NumeroHabitacion,
            Stay = $"{Ui.ShortDate(cuenta.FechaEntrada)} a {Ui.LongDate(cuenta.FechaSalida)}",
            Nights = cuenta.Noches,
            PricePerNight = cuenta.PrecioNoche,
            Consumptions = cuenta.Consumos.Select(c => c.AWeb()).ToList(),
            Tours = cuenta.Tours.Select(t => t.AWeb()).ToList(),
            ShowInvoiceModal = factura,
        });
    }
}
