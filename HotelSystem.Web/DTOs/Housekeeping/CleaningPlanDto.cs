using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Housekeeping;

/// <summary>Plan de limpieza priorizado por llegadas del dia (HU-11, RF-10).</summary>
public class CleaningPlanDto
{
    public IReadOnlyList<CleaningTaskDto> Tasks { get; set; } = [];
    public int ArrivalsTodayCount { get; set; }
    public int PendingCount { get; set; }
}

public record CleaningTaskDto(RoomDto Room, bool IsUrgent);
