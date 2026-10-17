using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Huesped;

/// <summary>Inicio del huesped: reservas y cancelacion (HU-03).</summary>
public class GuestHomeDto
{
    public string GuestName { get; set; } = "";
    public GuestReservationDto? Active { get; set; }
    public IReadOnlyList<GuestReservationDto> Reservations { get; set; } = [];
    public GuestReservationDto? CancelTarget { get; set; }
}
