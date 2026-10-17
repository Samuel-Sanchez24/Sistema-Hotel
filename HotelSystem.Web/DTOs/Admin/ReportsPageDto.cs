using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Admin;

/// <summary>Reportes de gestion: ocupacion, ingresos, servicios y personal (HU-13).</summary>
public class ReportsPageDto
{
    public ReportFilterDto Filter { get; set; } = new();

    public int TotalRooms { get; set; }
    public int OccupiedCount { get; set; }
    public int AvailableCount { get; set; }
    public int OccupancyRate => TotalRooms == 0 ? 0 : (int)Math.Round(OccupiedCount * 100.0 / TotalRooms);
    public IReadOnlyList<OccupancyRowDto> Occupancy { get; set; } = [];

    public IReadOnlyList<IncomeRowDto> Income { get; set; } = [];
    public decimal MaxIncome => Math.Max(Income.Count == 0 ? 1 : Income.Max(i => i.Amount), 1);
    public decimal IncomeTotal => Income.Sum(i => i.Amount);

    public IReadOnlyList<ServiceReportRowDto> Services { get; set; } = [];
    public IReadOnlyList<StaffReportRowDto> StaffRows { get; set; } = [];
}

public record OccupancyRowDto(string Label, int Total, int Occupied)
{
    public int Percent => (int)Math.Round(Occupied * 100.0 / Total);
}

public record IncomeRowDto(string Label, decimal Amount);

public record ServiceReportRowDto(string Label, int Count, decimal Total);

public record StaffReportRowDto(string Name, Role Role, bool Active, int Operations);
