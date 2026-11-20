using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Huesped;

/// <summary>Asistente de reserva de habitacion en maximo 3 pasos (HU-03, RNF-04).</summary>
public class BookRoomPageDto
{
    public int Step { get; set; } = 1;
    public BookRoomDto Form { get; set; } = new();

    /// <summary>Habitaciones del tipo elegido libres en todo el rango de fechas (RN-01).</summary>
    public IReadOnlyList<RoomDto> Available { get; set; } = [];
    public RoomDto? SelectedRoom { get; set; }

    /// <summary>Noches entre la fecha de entrada y la de salida.</summary>
    public int Nights { get; set; }
    public decimal Total => SelectedRoom is null ? 0 : SelectedRoom.Price * Nights;

    /// <summary>Fechas ya interpretadas del formulario.</summary>
    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }

    /// <summary>Fechas en el formato del prototipo, para mostrarlas.</summary>
    public string CheckInLabel { get; set; } = "";
    public string CheckOutLabel { get; set; } = "";

    /// <summary>Fecha minima que aceptan los campos de fecha (hoy), en formato AAAA-MM-DD.</summary>
    public string MinDate { get; set; } = "";

    /// <summary>Tarifa por noche y capacidad de cada tipo de habitacion, leidas de la base de datos.</summary>
    public IReadOnlyDictionary<RoomType, decimal> PriceByType { get; set; } = new Dictionary<RoomType, decimal>();
    public IReadOnlyDictionary<RoomType, int> CapacityByType { get; set; } = new Dictionary<RoomType, int>();

    /// <summary>Mensaje cuando las fechas no son validas (paso 1).</summary>
    public string? DateError { get; set; }

    /// <summary>Mensaje de la regla incumplida al confirmar (paso 3).</summary>
    public string? Error { get; set; }

    public bool Done { get; set; }
    public string? ReservationCode { get; set; }
    public string GuestEmail { get; set; } = "";

    /// <summary>Falso si la reserva quedo confirmada pero el correo no se pudo enviar (HU-04).</summary>
    public bool EmailSent { get; set; }
}
