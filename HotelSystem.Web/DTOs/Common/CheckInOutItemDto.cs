namespace HotelSystem.Web.DTOs.Common;

/// <summary>Llegada o salida programada para el dia (HU-06).</summary>
public class CheckInOutItemDto
{
    public string Id { get; set; } = "";
    public string GuestName { get; set; } = "";
    public string RoomNumber { get; set; } = "";
    public int Nights { get; set; }
    public string CheckIn { get; set; } = "";
    public string CheckOut { get; set; } = "";
}
