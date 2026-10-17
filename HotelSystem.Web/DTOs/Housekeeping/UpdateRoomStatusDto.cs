using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Housekeeping;

public class UpdateRoomStatusDto
{
    public int RoomId { get; set; }
    public RoomStatus NewStatus { get; set; }
    public string Filter { get; set; } = "all";
}
