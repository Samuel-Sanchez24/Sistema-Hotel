namespace HotelSystem.Web.DTOs.Common;

public class ConsumptionDto
{
    public int Id { get; set; }
    public string RoomNumber { get; set; } = "";
    public string GuestName { get; set; } = "";
    public string Service { get; set; } = "";
    public decimal Amount { get; set; }
    public string Date { get; set; } = "";
    public string Time { get; set; } = "";
}
