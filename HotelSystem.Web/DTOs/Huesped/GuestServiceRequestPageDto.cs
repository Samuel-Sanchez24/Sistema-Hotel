using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Huesped;

/// <summary>Solicitud de servicios del catalogo (HU-07).</summary>
public class GuestServiceRequestPageDto
{
    public string RoomNumber { get; set; } = "";
    public string GuestName { get; set; } = "";
    public IReadOnlyList<IGrouping<string, ServiceCatalogItemDto>> Categories { get; set; } = [];
    public ServiceCatalogItemDto? Selected { get; set; }
    public IReadOnlyList<ServiceRequestDto> Requests { get; set; } = [];
}
