using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Recepcion;

/// <summary>Prefactura, pago y cierre de cuenta (HU-06, HU-12, RN-07).</summary>
public class BillingPageDto
{
    public string? Query { get; set; }
    public IReadOnlyList<RoomDto> Results { get; set; } = [];
    public RoomDto? Selected { get; set; }
    public IReadOnlyList<ConsumptionDto> Consumptions { get; set; } = [];
    public int Nights { get; set; }
    public decimal RoomTotal { get; set; }
    public decimal ServicesTotal { get; set; }
    public decimal GrandTotal => RoomTotal + ServicesTotal;
    public bool Generated { get; set; }
    public bool Closed { get; set; }
    public string? PayMethod { get; set; }
    public string? PaidAmount { get; set; }
    public string? Error { get; set; }
    public IReadOnlyList<string> PaymentMethods { get; set; } = [];
}
