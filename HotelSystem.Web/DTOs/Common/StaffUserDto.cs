namespace HotelSystem.Web.DTOs.Common;

public class StaffUserDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public Role Role { get; set; }
    public bool Active { get; set; }
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}
