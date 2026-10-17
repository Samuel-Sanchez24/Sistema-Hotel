using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Huesped;

/// <summary>Resumen de prefactura del huesped (HU-12).</summary>
public class GuestBillDto
{
    public string GuestName { get; set; } = "";
    public string RoomNumber { get; set; } = "";
    public string Stay { get; set; } = "";
    public int Nights { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal RoomTotal => Nights * PricePerNight;
    public IReadOnlyList<ConsumptionDto> Consumptions { get; set; } = [];
    public decimal ServicesTotal => Consumptions.Sum(c => c.Amount);
    public decimal Total => RoomTotal + ServicesTotal;
    public bool ShowInvoiceModal { get; set; }
}
