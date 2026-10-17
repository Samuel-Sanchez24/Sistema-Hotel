namespace HotelSystem.Web.DTOs.Housekeeping;

public class ReportIncidentDto
{
    public int RoomId { get; set; }
    public string? Note { get; set; }
    public string Filter { get; set; } = "all";
}
