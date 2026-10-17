using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Huesped;

/// <summary>Asistente de reserva de habitacion en maximo 3 pasos (HU-03, RNF-04).</summary>
public class BookRoomPageDto
{
    public int Step { get; set; } = 1;
    public BookRoomDto Form { get; set; } = new();
    public IReadOnlyList<RoomDto> Available { get; set; } = [];
    public RoomDto? SelectedRoom { get; set; }
    public int Nights { get; set; } = 3;
    public decimal Total => SelectedRoom is null ? 0 : SelectedRoom.Price * Nights;
    public string? Error { get; set; }
    public bool Done { get; set; }
    public string GuestEmail { get; set; } = "";
}
