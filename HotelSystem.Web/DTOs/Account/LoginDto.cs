using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Account;

public class LoginDto
{
    public Role Role { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
