namespace HotelSystem.Web.DTOs.Common;

public class ErrorDto
{
    public string? RequestId { get; set; }
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
