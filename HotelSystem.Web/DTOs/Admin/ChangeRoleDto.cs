using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Admin;

public class ChangeRoleDto
{
    public int UserId { get; set; }
    public Role Role { get; set; }
}
