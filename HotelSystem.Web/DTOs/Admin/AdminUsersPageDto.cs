using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Admin;

/// <summary>Gestion de cuentas del personal (HU-01, RN-08).</summary>
public class AdminUsersPageDto
{
    public IReadOnlyList<StaffUserDto> Staff { get; set; } = [];
    public bool ShowForm { get; set; }
    public int? EditId { get; set; }
    public CreateStaffUserDto Form { get; set; } = new();
    public string? FormError { get; set; }
    public IReadOnlyList<Role> AssignableRoles { get; set; } = [];
}
