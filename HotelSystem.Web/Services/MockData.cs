using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.Services;

/// <summary>
/// Datos de demostracion del prototipo de alta fidelidad (Figma Make).
/// Se reemplazaran por la capa de Persistencia cuando este disponible.
/// </summary>
public static class MockData
{
    public const string Today = "9 Sep";

    public const string GuestName = "Valentina Lagos";
    public const string GuestEmail = "valentina.lagos@email.com";
    public const string GuestRoom = "408";
    public const string GuestUsername = "valentina.lagos";
    public const string GuestPassword = "guest2026";

    public static readonly string[] Guides = ["Alberto Cruz", "Mario Rios", "Elena Mora", "Carmen Vega", "Diego Pardo"];
    public static readonly string[] Vehicles = ["Van #001", "Van #002", "Van #003", "Bus #003", "Lancha #01", "Lancha #02"];

    /// <summary>Habitaciones con llegada hoy: van primero en el plan de limpieza.</summary>
    public static readonly string[] ArrivalsToday = ["305", "501"];

    public static readonly string[] PaymentMethods =
        ["Efectivo", "Datafono - Visa **** 4242", "Datafono - Mastercard **** 5555", "Transferencia bancaria"];

    public static readonly Dictionary<RoomType, decimal> PriceByType = new()
    {
        [RoomType.Simple] = 120000, [RoomType.Doble] = 150000, [RoomType.Suite] = 250000,
    };

    public static readonly Dictionary<RoomType, int> CapacityByType = new()
    {
        [RoomType.Simple] = 2, [RoomType.Doble] = 3, [RoomType.Suite] = 4,
    };

    /// <summary>Usuario demo y nombre mostrado para cada perfil de la pantalla de login.</summary>
    public static readonly Dictionary<Role, (string Username, string Password, string Display)> DemoCredentials = new()
    {
        [Role.Huesped] = (GuestUsername, GuestPassword, GuestName),
        [Role.Recepcion] = ("mlopez", "recep2026", "Maria Lopez"),
        [Role.Housekeeping] = ("amartinez", "hk2026", "Ana Martinez"),
        [Role.Coordinador] = ("ltorres", "coord2026", "Luis Torres"),
        [Role.Servicios] = ("pgomez", "serv2026", "Paula Gomez"),
        [Role.Admin] = ("rsilva", "admin2026", "Roberto Silva"),
    };

    // numero, estado, hora de actualizacion, huesped, check-in, check-out
    private static readonly (string Num, RoomStatus Status, string Upd, string? Guest, string? In, string? Out)[] RawRooms =
    [
        ("101", RoomStatus.Disponible, "09:12", null, null, null), ("102", RoomStatus.Ocupada, "07:30", "Carlos Mejia", "5 Sep", "10 Sep"),
        ("103", RoomStatus.Disponible, "08:45", null, null, null), ("104", RoomStatus.Disponible, "10:00", null, null, null),
        ("105", RoomStatus.EnLimpieza, "11:02", null, null, null), ("106", RoomStatus.Disponible, "09:30", null, null, null),
        ("107", RoomStatus.Ocupada, "08:00", "Sandra Velez", "7 Sep", "10 Sep"), ("108", RoomStatus.Disponible, "10:15", null, null, null),
        ("109", RoomStatus.Mantenimiento, "06:00", null, null, null), ("110", RoomStatus.Disponible, "09:50", null, null, null),
        ("201", RoomStatus.Disponible, "09:10", null, null, null), ("202", RoomStatus.Ocupada, "07:45", "Familia Ramos", "6 Sep", "11 Sep"),
        ("203", RoomStatus.EnLimpieza, "11:30", null, null, null), ("204", RoomStatus.Disponible, "10:05", null, null, null),
        ("205", RoomStatus.Ocupada, "08:20", "Maria Torres", "8 Sep", "12 Sep"), ("206", RoomStatus.Disponible, "09:55", null, null, null),
        ("207", RoomStatus.Disponible, "10:10", null, null, null), ("208", RoomStatus.Ocupada, "07:55", "Pedro Salinas", "7 Sep", "10 Sep"),
        ("209", RoomStatus.Disponible, "10:20", null, null, null), ("210", RoomStatus.Disponible, "09:40", null, null, null),
        ("301", RoomStatus.Disponible, "09:05", null, null, null), ("302", RoomStatus.Disponible, "09:48", null, null, null),
        ("303", RoomStatus.Ocupada, "07:35", "Familia Gutierrez", "5 Sep", "10 Sep"), ("304", RoomStatus.Mantenimiento, "05:30", null, null, null),
        ("305", RoomStatus.Disponible, "10:25", null, null, null), ("306", RoomStatus.Ocupada, "08:10", "Andres Castro", "8 Sep", "11 Sep"),
        ("307", RoomStatus.Disponible, "09:52", null, null, null), ("308", RoomStatus.Disponible, "10:30", null, null, null),
        ("309", RoomStatus.EnLimpieza, "11:45", null, null, null), ("310", RoomStatus.Disponible, "10:00", null, null, null),
        ("401", RoomStatus.Ocupada, "07:40", "Laura Pena", "6 Sep", "10 Sep"), ("402", RoomStatus.Disponible, "09:15", null, null, null),
        ("403", RoomStatus.Disponible, "09:30", null, null, null), ("404", RoomStatus.Ocupada, "07:50", "Jorge Ruiz", "7 Sep", "12 Sep"),
        ("405", RoomStatus.Disponible, "10:00", null, null, null), ("406", RoomStatus.Disponible, "09:45", null, null, null),
        ("407", RoomStatus.Disponible, "10:05", null, null, null), ("408", RoomStatus.Ocupada, "08:05", "Valentina Lagos", "9 Sep", "13 Sep"),
        ("409", RoomStatus.Disponible, "09:20", null, null, null), ("410", RoomStatus.EnLimpieza, "11:15", null, null, null),
        ("501", RoomStatus.Disponible, "09:25", null, null, null), ("502", RoomStatus.Ocupada, "07:25", "Familia Morales", "5 Sep", "12 Sep"),
        ("503", RoomStatus.Disponible, "09:50", null, null, null), ("504", RoomStatus.Disponible, "10:10", null, null, null),
        ("505", RoomStatus.Ocupada, "08:15", "Gustavo Vargas", "9 Sep", "11 Sep"), ("506", RoomStatus.Disponible, "10:20", null, null, null),
        ("507", RoomStatus.Disponible, "09:35", null, null, null), ("508", RoomStatus.Disponible, "10:40", null, null, null),
        ("509", RoomStatus.Mantenimiento, "04:00", null, null, null), ("510", RoomStatus.Disponible, "09:55", null, null, null),
        ("601", RoomStatus.Ocupada, "08:00", "Luna de Miel Diaz", "8 Sep", "14 Sep"), ("602", RoomStatus.Disponible, "10:00", null, null, null),
        ("603", RoomStatus.Disponible, "09:45", null, null, null), ("604", RoomStatus.Disponible, "10:05", null, null, null),
        ("605", RoomStatus.Disponible, "09:30", null, null, null), ("606", RoomStatus.Disponible, "10:15", null, null, null),
        ("607", RoomStatus.Disponible, "09:55", null, null, null), ("608", RoomStatus.Disponible, "10:20", null, null, null),
        ("609", RoomStatus.Mantenimiento, "03:30", null, null, null), ("610", RoomStatus.Disponible, "10:00", null, null, null),
    ];

    private static readonly string[] DamagedRooms = ["109", "304", "509", "609"];

    /// <summary>Pisos 1-2 simples, 3-4 dobles, 5-6 suites.</summary>
    private static RoomType TypeForFloor(int floor) => floor switch
    {
        <= 2 => RoomType.Simple,
        <= 4 => RoomType.Doble,
        _ => RoomType.Suite,
    };

    public static List<RoomDto> Rooms() => RawRooms.Select((r, i) =>
    {
        var floor = r.Num[0] - '0';
        var type = TypeForFloor(floor);
        return new RoomDto
        {
            Id = i + 1, Number = r.Num, Type = type, Status = r.Status, Floor = floor,
            Capacity = CapacityByType[type], Price = PriceByType[type],
            GuestName = r.Guest, CheckIn = r.In, CheckOut = r.Out,
            LastUpdated = $"{Today} - {r.Upd}",
            MaintenanceNote = DamagedRooms.Contains(r.Num) ? "Dano reportado al ingresar turno" : null,
        };
    }).ToList();

    public static List<TourDto> Tours() =>
    [
        new() { Id = 1, Name = "Playa Volcanica", Destination = "Playa El Zafiro", Date = "10 Sep", Time = "8:00 AM", Capacity = 15, Booked = 12, Guide = "Alberto Cruz", Vehicle = "Van #001", Price = 85000 },
        new() { Id = 2, Name = "Reserva Natural", Destination = "Reserva El Mangle", Date = "10 Sep", Time = "7:00 AM", Capacity = 20, Booked = 8, Price = 70000 },
        new() { Id = 3, Name = "Paseo en Lancha", Destination = "Bahia Cristal", Date = "9 Sep", Time = "3:00 PM", Capacity = 12, Booked = 12, Guide = "Mario Rios", Vehicle = "Lancha #02", Price = 95000 },
        new() { Id = 4, Name = "City Tour Costero", Destination = "Centro Historico", Date = "11 Sep", Time = "9:00 AM", Capacity = 25, Booked = 18, Vehicle = "Bus #003", Price = 55000 },
        new() { Id = 5, Name = "Senderismo Sierra", Destination = "Parque Sierra Alta", Date = "10 Sep", Time = "6:00 AM", Capacity = 10, Booked = 5, Guide = "Elena Mora", Vehicle = "Van #002", Price = 75000 },
    ];

    public static List<ConsumptionDto> Consumptions() =>
    [
        new() { Id = 1, RoomNumber = "205", GuestName = "Maria Torres", Service = "Restaurante", Amount = 45000, Date = "8 Sep", Time = "13:20" },
        new() { Id = 2, RoomNumber = "205", GuestName = "Maria Torres", Service = "Spa", Amount = 80000, Date = "9 Sep", Time = "10:00" },
        new() { Id = 3, RoomNumber = "306", GuestName = "Andres Castro", Service = "Lavanderia", Amount = 25000, Date = "9 Sep", Time = "09:30" },
        new() { Id = 4, RoomNumber = "408", GuestName = "Valentina Lagos", Service = "Bar", Amount = 35000, Date = "9 Sep", Time = "21:00" },
        new() { Id = 5, RoomNumber = "408", GuestName = "Valentina Lagos", Service = "Transporte Aeropuerto", Amount = 60000, Date = "10 Sep", Time = "06:00" },
    ];

    public static List<StaffUserDto> Staff() =>
    [
        new() { Id = 1, Name = "Roberto Silva", Email = "rsilva@hotel.com", Role = Role.Admin, Active = true, Username = "rsilva", Password = "admin2026" },
        new() { Id = 2, Name = "Maria Lopez", Email = "mlopez@hotel.com", Role = Role.Recepcion, Active = true, Username = "mlopez", Password = "recep2026" },
        new() { Id = 3, Name = "Carlos Ruiz", Email = "cruiz@hotel.com", Role = Role.Recepcion, Active = true, Username = "cruiz", Password = "recep2026" },
        new() { Id = 4, Name = "Ana Martinez", Email = "amartinez@hotel.com", Role = Role.Housekeeping, Active = true, Username = "amartinez", Password = "hk2026" },
        new() { Id = 5, Name = "Luis Torres", Email = "ltorres@hotel.com", Role = Role.Coordinador, Active = true, Username = "ltorres", Password = "coord2026" },
        new() { Id = 6, Name = "Paula Gomez", Email = "pgomez@hotel.com", Role = Role.Servicios, Active = true, Username = "pgomez", Password = "serv2026" },
        new() { Id = 7, Name = "Juan Herrera", Email = "jherrera@hotel.com", Role = Role.Housekeeping, Active = false, Username = "jherrera", Password = "hk2026" },
    ];

    public static List<ServiceCatalogItemDto> Catalog() =>
    [
        new() { Id = 1, Name = "Restaurante", Category = "Gastronomia", Description = "Servicio de restaurante en el hotel", Active = true },
        new() { Id = 2, Name = "Spa", Category = "Bienestar", Description = "Masajes y tratamientos de spa", Price = 80000, Active = true },
        new() { Id = 3, Name = "Bar", Category = "Gastronomia", Description = "Bar y bebidas", Active = true },
        new() { Id = 4, Name = "Lavanderia", Category = "Habitacion", Description = "Servicio de lavado y planchado de ropa", Price = 25000, Active = true },
        new() { Id = 5, Name = "Transporte Aeropuerto", Category = "Transporte", Description = "Traslado al aeropuerto internacional", Price = 60000, Active = true },
        new() { Id = 6, Name = "Room Service", Category = "Gastronomia", Description = "Servicio de comidas a la habitacion", Active = true },
        new() { Id = 7, Name = "Minibar", Category = "Gastronomia", Description = "Recarga de minibar", Active = false },
        new() { Id = 8, Name = "Tour Privado", Category = "Actividades", Description = "Excursiones personalizadas", Price = 150000, Active = true },
    ];

    public static List<GuestReservationDto> GuestReservations() =>
    [
        new() { Id = "RES-048", Kind = ReservationKind.Habitacion, Status = ReservationStatus.EnCurso, RoomType = RoomType.Doble, RoomNumber = "408", CheckIn = "9 Sep", CheckOut = "13 Sep", Nights = 4, PricePerNight = 150000, Cancellable = false, CancelReason = "El check-in ya fue registrado. No es posible cancelar una estancia en curso." },
        new() { Id = "RES-049", Kind = ReservationKind.Habitacion, Status = ReservationStatus.Confirmada, RoomType = RoomType.Simple, RoomNumber = "210", CheckIn = "11 Sep", CheckOut = "13 Sep", Nights = 2, PricePerNight = 120000, Cancellable = true, LateCancel = true },
        new() { Id = "RES-052", Kind = ReservationKind.Habitacion, Status = ReservationStatus.Confirmada, RoomType = RoomType.Suite, CheckIn = "15 Dic", CheckOut = "20 Dic", Nights = 5, PricePerNight = 250000, Cancellable = true, LateCancel = false },
        new() { Id = "RES-033", Kind = ReservationKind.Tour, Status = ReservationStatus.Confirmada, TourName = "Senderismo Sierra", TourDate = "10 Sep", Persons = 2, TourPrice = 75000, Cancellable = false, CancelReason = "El tour es manana. No se permiten cancelaciones con menos de 24h de anticipacion." },
    ];

    public static List<CheckInOutItemDto> TodayCheckIns() =>
    [
        new() { Id = "RES-041", GuestName = "Esperanza Rios", RoomNumber = "305", Nights = 3, CheckIn = "9 Sep", CheckOut = "12 Sep" },
        new() { Id = "RES-042", GuestName = "Familia Pedraza", RoomNumber = "501", Nights = 5, CheckIn = "9 Sep", CheckOut = "14 Sep" },
    ];

    public static List<CheckInOutItemDto> TodayCheckOuts() =>
    [
        new() { Id = "RES-018", GuestName = "Carlos Mejia", RoomNumber = "102", Nights = 5, CheckIn = "5 Sep", CheckOut = "10 Sep" },
        new() { Id = "RES-027", GuestName = "Familia Gutierrez", RoomNumber = "303", Nights = 4, CheckIn = "5 Sep", CheckOut = "10 Sep" },
    ];
}
