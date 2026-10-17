using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Tours;

/// <summary>Asignacion de guias y vehiculos (HU-10, RN-03).</summary>
public class TourAssignmentPageDto
{
    public IReadOnlyList<TourDto> Tours { get; set; } = [];
    public TourDto? Selected { get; set; }
    public IReadOnlyList<ResourceOptionDto> Guides { get; set; } = [];
    public IReadOnlyList<ResourceOptionDto> Vehicles { get; set; } = [];
    public string? Guide { get; set; }
    public string? Vehicle { get; set; }
    public string? Error { get; set; }
}

/// <summary>Guia o vehiculo, marcado como ocupado si ya esta asignado en el mismo horario.</summary>
public record ResourceOptionDto(string Name, bool Busy);
