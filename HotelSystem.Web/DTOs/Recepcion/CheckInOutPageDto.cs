using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Recepcion;

/// <summary>Operaciones de check-in / check-out del dia (HU-06).</summary>
public class CheckInOutPageDto
{
    public string Tab { get; set; } = "checkin";
    public string? Query { get; set; }
    public IReadOnlyList<CheckInOutItemDto> Items { get; set; } = [];
    public int CheckInCount { get; set; }
    public int CheckOutCount { get; set; }
    public bool IsCheckIn => Tab == "checkin";
}
