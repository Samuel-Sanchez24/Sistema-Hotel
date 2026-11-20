using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Admin;

/// <summary>Reportes de gestion: ocupacion, ingresos, servicios y personal (HU-13).</summary>
public class ReportsPageDto
{
    public ReportFilterDto Filter { get; set; } = new();

    /// <summary>Periodo consultado en texto, por ejemplo "1 Nov 2026 a 16 Nov 2026".</summary>
    public string PeriodLabel { get; set; } = "";

    /// <summary>Mensaje cuando el rango de fechas no es valido.</summary>
    public string? Error { get; set; }

    /// <summary>Porcentaje de noches-habitacion ocupadas en el periodo.</summary>
    public int OccupancyRate { get; set; }
    public int OccupiedNights { get; set; }
    public int AvailableNights { get; set; }

    /// <summary>Estado actual del hotel, independiente del periodo.</summary>
    public int TotalRooms { get; set; }
    public int OccupiedNow { get; set; }
    public int AvailableNow { get; set; }

    public IReadOnlyList<OccupancyRowDto> Occupancy { get; set; } = [];
    public IReadOnlyList<IncomeRowDto> Income { get; set; } = [];
    public decimal MaxIncome => Math.Max(Income.Count == 0 ? 1 : Income.Max(i => i.Amount), 1);
    public decimal IncomeTotal => Income.Sum(i => i.Amount);
    public IReadOnlyList<ServiceReportRowDto> Services { get; set; } = [];
    public IReadOnlyList<StaffReportRowDto> StaffRows { get; set; } = [];
}

/// <summary>Ocupacion de un tipo de habitacion en noches-habitacion del periodo.</summary>
public record OccupancyRowDto(string Label, int Total, int Occupied)
{
    public int Percent => Total == 0 ? 0 : (int)Math.Round(Occupied * 100.0 / Total);
}

public record IncomeRowDto(string Label, decimal Amount);

public record ServiceReportRowDto(string Label, int Count, decimal Total);

public record StaffReportRowDto(string Name, Role Role, bool Active, int Operations);
