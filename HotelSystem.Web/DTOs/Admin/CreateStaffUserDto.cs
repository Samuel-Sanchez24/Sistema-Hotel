using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Admin;

public class CreateStaffUserDto
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public Role Role { get; set; } = Role.Recepcion;
}
