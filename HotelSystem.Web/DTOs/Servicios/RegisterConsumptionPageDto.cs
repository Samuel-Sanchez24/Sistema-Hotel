using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Servicios;

/// <summary>Registro de consumos adicionales (HU-08, RN-06).</summary>
public class RegisterConsumptionPageDto
{
    public IReadOnlyList<RoomDto> OccupiedRooms { get; set; } = [];
    public IReadOnlyList<ServiceCatalogItemDto> ActiveServices { get; set; } = [];
    public RoomDto? SelectedRoom { get; set; }
    public string? RoomNumber { get; set; }
    public string? Service { get; set; }
    public string? Amount { get; set; }

    /// <summary>Fecha del consumo (AAAA-MM-DD). Por defecto, hoy.</summary>
    public string Date { get; set; } = "";
    public string? Error { get; set; }
}
