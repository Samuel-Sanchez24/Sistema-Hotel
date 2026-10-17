using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Recepcion;

/// <summary>Panel de habitaciones en tiempo real (HU-05).</summary>
public class RoomsPanelDto
{
    public int Floor { get; set; } = 1;
    public RoomStatus? Filter { get; set; }
    public IReadOnlyList<RoomDto> FloorRooms { get; set; } = [];
    public IReadOnlyDictionary<RoomStatus, int> Counts { get; set; } = new Dictionary<RoomStatus, int>();
    public RoomDto? Selected { get; set; }
}
