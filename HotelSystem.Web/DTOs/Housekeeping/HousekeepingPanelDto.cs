using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Housekeeping;

/// <summary>Estado de habitaciones e incidencias (HU-11).</summary>
public class HousekeepingPanelDto
{
    public string Filter { get; set; } = "all";
    public IReadOnlyList<RoomDto> Rooms { get; set; } = [];
    public int CleaningCount { get; set; }
    public int MaintenanceCount { get; set; }
    public RoomDto? IncidentRoom { get; set; }
    public string? IncidentNote { get; set; }
    public string? IncidentError { get; set; }
}
