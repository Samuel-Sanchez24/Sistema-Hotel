using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Huesped;

/// <summary>Resumen de prefactura del huesped (HU-12): alojamiento, servicios y tours (RN-07).</summary>
public class GuestBillDto
{
    /// <summary>Falso si el huesped no tiene una estancia en curso: no hay cuenta que mostrar.</summary>
    public bool HasStay { get; set; } = true;

    public string GuestName { get; set; } = "";
    public string RoomNumber { get; set; } = "";
    public string Stay { get; set; } = "";
    public int Nights { get; set; }
    public decimal PricePerNight { get; set; }
    public decimal RoomTotal => Nights * PricePerNight;
    public IReadOnlyList<ConsumptionDto> Consumptions { get; set; } = [];
    public decimal ServicesTotal => Consumptions.Sum(c => c.Amount);
    public IReadOnlyList<BillTourDto> Tours { get; set; } = [];
    public decimal ToursTotal => Tours.Sum(t => t.Total);
    public decimal Total => RoomTotal + ServicesTotal + ToursTotal;
    public bool ShowInvoiceModal { get; set; }
}
