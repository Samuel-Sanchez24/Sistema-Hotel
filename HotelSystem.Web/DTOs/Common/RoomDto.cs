namespace HotelSystem.Web.DTOs.Common;

public class RoomDto
{
    public int Id { get; set; }
    public string Number { get; set; } = "";
    public RoomType Type { get; set; }
    public RoomStatus Status { get; set; }
    public int Floor { get; set; }
    public int Capacity { get; set; }
    public decimal Price { get; set; }
    public string? GuestName { get; set; }
    public string? CheckIn { get; set; }
    public string? CheckOut { get; set; }
    public string LastUpdated { get; set; } = "";
    public string? MaintenanceNote { get; set; }
}
