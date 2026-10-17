using HotelSystem.Web.DTOs.Account;
using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.Helpers;

namespace HotelSystem.Web.Services;

/// <summary>
/// Estado compartido en memoria (equivale al useState del prototipo React).
/// Se registra como Singleton: los cambios se ven entre roles hasta reiniciar la app.
/// </summary>
public class InMemoryHotelDataService : IHotelDataService
{
    private readonly object _sync = new();
    private readonly List<RoomDto> _rooms = MockData.Rooms();
    private readonly List<TourDto> _tours = MockData.Tours();
    private readonly List<ConsumptionDto> _consumptions = MockData.Consumptions();
    private readonly List<StaffUserDto> _staff = MockData.Staff();
    private readonly List<ServiceCatalogItemDto> _catalog = MockData.Catalog();
    private readonly List<GuestReservationDto> _reservations = MockData.GuestReservations();
    private readonly List<ServiceRequestDto> _serviceRequests = [];
    private readonly List<CheckInOutItemDto> _checkIns = MockData.TodayCheckIns();
    private readonly List<CheckInOutItemDto> _checkOuts = MockData.TodayCheckOuts();
    private readonly HashSet<int> _guestBookedTours = [];

    private static string Stamp() => $"{MockData.Today} - {Ui.Now()}";

    /* ─── HU-02: Inicio de sesion ─── */

    public LoginResultDto ValidateLogin(LoginDto login)
    {
        const string invalid = "Credenciales incorrectas. Verifique los datos ingresados. (HU-02 Esc.2)";

        if (login.Role == Role.Huesped)
        {
            return login.Username == MockData.GuestUsername && login.Password == MockData.GuestPassword
                ? new(true, null, MockData.GuestName)
                : new(false, invalid);
        }

        lock (_sync)
        {
            var user = _staff.FirstOrDefault(u => u.Username == login.Username);
            if (user is { Active: false })
                return new(false, "Cuenta desactivada. Comuniquese con administracion para reactivar su acceso. (HU-02 Esc.3)");
            if (user is null || user.Password != login.Password || user.Role != login.Role)
                return new(false, invalid);
            return new(true, null, user.Name);
        }
    }

    /* ─── Habitaciones ─── */

    public IReadOnlyList<RoomDto> GetRooms() { lock (_sync) return _rooms.ToList(); }

    public RoomDto? GetRoom(int id) { lock (_sync) return _rooms.FirstOrDefault(r => r.Id == id); }

    public void SetRoomStatus(int roomId, RoomStatus status, string? maintenanceNote = null)
    {
        lock (_sync)
        {
            var room = _rooms.FirstOrDefault(r => r.Id == roomId);
            if (room is null) return;
            room.Status = status;
            room.LastUpdated = Stamp();
            if (!string.IsNullOrWhiteSpace(maintenanceNote)) room.MaintenanceNote = maintenanceNote;
        }
    }

    public IReadOnlyList<CheckInOutItemDto> GetTodayCheckIns() { lock (_sync) return _checkIns.ToList(); }

    public IReadOnlyList<CheckInOutItemDto> GetTodayCheckOuts() { lock (_sync) return _checkOuts.ToList(); }

    public CheckInOutItemDto? RegisterCheckIn(string reservationId)
    {
        lock (_sync)
        {
            var item = _checkIns.FirstOrDefault(c => c.Id == reservationId);
            var room = item is null ? null : _rooms.FirstOrDefault(r => r.Number == item.RoomNumber);
            if (item is null || room is null) return null;
            room.Status = RoomStatus.Ocupada;
            room.GuestName = item.GuestName;
            room.CheckIn = item.CheckIn;
            room.CheckOut = item.CheckOut;
            room.LastUpdated = Stamp();
            return item;
        }
    }

    /// <summary>Cierre de cuenta (RN-07): la habitacion pasa a limpieza y se libera.</summary>
    public void CloseAccount(int roomId)
    {
        lock (_sync)
        {
            var room = _rooms.FirstOrDefault(r => r.Id == roomId);
            if (room is null) return;
            room.Status = RoomStatus.EnLimpieza;
            room.GuestName = null;
            room.CheckIn = null;
            room.CheckOut = null;
            room.LastUpdated = Stamp();
        }
    }

    /* ─── Tours ─── */

    public IReadOnlyList<TourDto> GetTours() { lock (_sync) return _tours.ToList(); }

    public TourDto? GetTour(int id) { lock (_sync) return _tours.FirstOrDefault(t => t.Id == id); }

    public void AdjustTourQuota(int tourId, int delta)
    {
        lock (_sync)
        {
            var t = _tours.FirstOrDefault(x => x.Id == tourId);
            if (t is not null) t.Booked = Math.Clamp(t.Booked + delta, 0, t.Capacity);
        }
    }

    /// <summary>RN-03: un guia o vehiculo no puede estar en dos tours del mismo horario.</summary>
    public OperationResultDto AssignTourResources(int tourId, string guide, string vehicle)
    {
        lock (_sync)
        {
            var tour = _tours.FirstOrDefault(t => t.Id == tourId);
            if (tour is null) return OperationResultDto.Fail("Complete todos los campos.");
            var sameSlot = _tours.Where(t => t.Id != tourId && t.Date == tour.Date && t.Time == tour.Time).ToList();
            if (sameSlot.Any(t => t.Guide == guide))
                return OperationResultDto.Fail($"{guide} ya esta asignado a otro tour en el mismo horario (RN-03 - HU-10 Esc.4).");
            if (sameSlot.Any(t => t.Vehicle == vehicle))
                return OperationResultDto.Fail($"{vehicle} ya esta asignado a otro tour en el mismo horario (RN-03 - HU-10 Esc.4).");
            tour.Guide = guide;
            tour.Vehicle = vehicle;
            return OperationResultDto.Ok($"Asignacion guardada: {guide} + {vehicle} - {tour.Name}");
        }
    }

    public TourDto? ReleaseTourResources(int tourId)
    {
        lock (_sync)
        {
            var tour = _tours.FirstOrDefault(t => t.Id == tourId);
            if (tour is null) return null;
            var released = new TourDto { Name = tour.Name, Guide = tour.Guide, Vehicle = tour.Vehicle };
            tour.Guide = "";
            tour.Vehicle = "";
            return released;
        }
    }

    public IReadOnlyList<string> GetGuides() => MockData.Guides;

    public IReadOnlyList<string> GetVehicles() => MockData.Vehicles;

    public OperationResultDto BookGuestTour(int tourId, int persons)
    {
        lock (_sync)
        {
            var tour = _tours.FirstOrDefault(t => t.Id == tourId);
            if (tour is null || persons > tour.FreeSpots)
                return OperationResultDto.Fail("No hay cupos suficientes. Seleccione otra fecha o tour (RN-11 - HU-09 Esc.2).");
            tour.Booked += persons;
            _guestBookedTours.Add(tourId);
            return OperationResultDto.Ok($"Tour reservado: {tour.Name} - {persons} persona(s)");
        }
    }

    public IReadOnlyCollection<int> GetGuestBookedTourIds() { lock (_sync) return _guestBookedTours.ToList(); }

    /* ─── Consumos y servicios ─── */

    public IReadOnlyList<ConsumptionDto> GetConsumptions() { lock (_sync) return _consumptions.ToList(); }

    public ConsumptionDto AddConsumption(string roomNumber, string service, decimal amount)
    {
        lock (_sync)
        {
            var room = _rooms.FirstOrDefault(r => r.Number == roomNumber);
            var c = new ConsumptionDto
            {
                Id = _consumptions.Count + 1, RoomNumber = roomNumber, GuestName = room?.GuestName ?? "Desconocido",
                Service = service, Amount = amount, Date = MockData.Today, Time = Ui.Now(),
            };
            _consumptions.Add(c);
            return c;
        }
    }

    public IReadOnlyList<ServiceCatalogItemDto> GetCatalog() => _catalog;

    public IReadOnlyList<ServiceRequestDto> GetServiceRequests() { lock (_sync) return _serviceRequests.ToList(); }

    public ServiceRequestDto? AddServiceRequest(int serviceId, string roomNumber)
    {
        lock (_sync)
        {
            var svc = _catalog.FirstOrDefault(s => s.Id == serviceId && s.Active);
            if (svc is null) return null;
            var req = new ServiceRequestDto
            {
                Id = _serviceRequests.Count + 1, ServiceId = svc.Id, ServiceName = svc.Name,
                RoomNumber = roomNumber, Status = ServiceRequestStatus.Pendiente, RequestedAt = Stamp(),
            };
            _serviceRequests.Add(req);
            return req;
        }
    }

    /* ─── Reservas del huesped ─── */

    public IReadOnlyList<GuestReservationDto> GetGuestReservations() { lock (_sync) return _reservations.ToList(); }

    public GuestReservationDto? CancelGuestReservation(string id)
    {
        lock (_sync)
        {
            var res = _reservations.FirstOrDefault(r => r.Id == id);
            if (res is null || !res.Cancellable || res.Status == ReservationStatus.Cancelada) return null;
            res.Status = ReservationStatus.Cancelada;
            return res;
        }
    }

    /* ─── Personal ─── */

    public IReadOnlyList<StaffUserDto> GetStaff() { lock (_sync) return _staff.ToList(); }

    public StaffUserDto CreateStaff(string name, string email, Role role)
    {
        lock (_sync)
        {
            var user = new StaffUserDto
            {
                Id = _staff.Count + 1, Name = name, Email = email, Role = role, Active = true,
                Username = email.Split('@')[0], Password = "temp2026",
            };
            _staff.Add(user);
            return user;
        }
    }

    public StaffUserDto? ToggleStaffActive(int id)
    {
        lock (_sync)
        {
            var user = _staff.FirstOrDefault(u => u.Id == id);
            if (user is not null) user.Active = !user.Active;
            return user;
        }
    }

    public void UpdateStaffRole(int id, Role role)
    {
        lock (_sync)
        {
            var user = _staff.FirstOrDefault(u => u.Id == id);
            if (user is not null) user.Role = role;
        }
    }
}
