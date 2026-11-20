using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Huesped;

public class BookRoomDto
{
    public RoomType Type { get; set; } = RoomType.Doble;

    /// <summary>Fecha de entrada en formato AAAA-MM-DD (campo de fecha HTML). Vacia = valor por defecto.</summary>
    public string CheckIn { get; set; } = "";

    /// <summary>Fecha de salida en formato AAAA-MM-DD.</summary>
    public string CheckOut { get; set; } = "";

    public int Guests { get; set; } = 2;
    public int? RoomId { get; set; }
    public string? PayMethod { get; set; }
    public bool AgeConfirmed { get; set; }
}
