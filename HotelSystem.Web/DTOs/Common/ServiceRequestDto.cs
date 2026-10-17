namespace HotelSystem.Web.DTOs.Common;

public class ServiceRequestDto
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = "";
    public string RoomNumber { get; set; } = "";
    public ServiceRequestStatus Status { get; set; }
    public string RequestedAt { get; set; } = "";
}
