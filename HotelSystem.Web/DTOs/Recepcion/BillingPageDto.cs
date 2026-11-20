using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Recepcion;

/// <summary>Prefactura, pago y cierre de cuenta (HU-06, HU-12, RN-07).</summary>
public class BillingPageDto
{
    public string? Query { get; set; }
    public IReadOnlyList<RoomDto> Results { get; set; } = [];
    public RoomDto? Selected { get; set; }
    public IReadOnlyList<ConsumptionDto> Consumptions { get; set; } = [];
    public IReadOnlyList<BillTourDto> Tours { get; set; } = [];

    /// <summary>Noches de la estancia segun las fechas de la reserva.</summary>
    public int Nights { get; set; }
    public decimal RoomTotal { get; set; }
    public decimal ServicesTotal { get; set; }
    public decimal ToursTotal { get; set; }

    /// <summary>Monto consolidado: alojamiento + servicios adicionales + tours.</summary>
    public decimal GrandTotal => RoomTotal + ServicesTotal + ToursTotal;
    public bool Generated { get; set; }
    public bool Closed { get; set; }

    /// <summary>Numero de la prefactura final, cuando la cuenta ya se cerro.</summary>
    public int? PrefacturaId { get; set; }
    public string? PayMethod { get; set; }
    public string? PaidAmount { get; set; }
    public string? Error { get; set; }
    public IReadOnlyList<string> PaymentMethods { get; set; } = [];
}
