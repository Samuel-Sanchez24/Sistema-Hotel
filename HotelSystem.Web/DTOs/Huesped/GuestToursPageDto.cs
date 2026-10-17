using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Huesped;

/// <summary>Reserva de tours por el huesped (HU-09, RN-05).</summary>
public class GuestToursPageDto
{
    public IReadOnlyList<TourDto> Tours { get; set; } = [];
    public IReadOnlyCollection<int> ConfirmedIds { get; set; } = [];
    public TourDto? Selected { get; set; }
    public BookTourDto Form { get; set; } = new();
    public string? Error { get; set; }
}
