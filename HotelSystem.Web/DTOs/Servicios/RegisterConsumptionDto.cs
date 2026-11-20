namespace HotelSystem.Web.DTOs.Servicios;

public class RegisterConsumptionDto
{
    public string? RoomNumber { get; set; }
    public string? Service { get; set; }
    public string? Amount { get; set; }

    /// <summary>Fecha del consumo en formato AAAA-MM-DD. Solo se acepta la de hoy (RN-06).</summary>
    public string? Date { get; set; }
}
