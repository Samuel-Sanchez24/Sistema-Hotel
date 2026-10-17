using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Huesped;

/// <summary>Consulta de consumos del huesped (HU-07, RN-07).</summary>
public class GuestConsumptionsDto
{
    public string RoomNumber { get; set; } = "";
    public IReadOnlyList<ConsumptionDto> Items { get; set; } = [];
    public decimal Total => Items.Sum(c => c.Amount);
}
