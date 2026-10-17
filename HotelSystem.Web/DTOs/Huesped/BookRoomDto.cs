using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Huesped;

public class BookRoomDto
{
    public RoomType Type { get; set; } = RoomType.Doble;
    public string CheckIn { get; set; } = "15 Sep 2026";
    public string CheckOut { get; set; } = "18 Sep 2026";
    public int Guests { get; set; } = 2;
    public int? RoomId { get; set; }
    public string? PayMethod { get; set; }
    public bool AgeConfirmed { get; set; }
}
