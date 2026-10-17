using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Tours;

/// <summary>Panel de cupos de tours (HU-10).</summary>
public class ToursPanelDto
{
    public IReadOnlyList<TourDto> Tours { get; set; } = [];
    public TourDto? Selected { get; set; }
}
