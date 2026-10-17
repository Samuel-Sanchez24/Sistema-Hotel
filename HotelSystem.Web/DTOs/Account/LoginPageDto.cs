using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.DTOs.Account;

public class LoginPageDto
{
    public IReadOnlyList<LoginRoleOptionDto> Roles { get; set; } = [];
    public Role? SelectedRole { get; set; }
    public string Username { get; set; } = "";
    public string DemoPassword { get; set; } = "";
    public string? Error { get; set; }
}

public record LoginRoleOptionDto(Role Role, string Label, string Description, string Icon, string DemoName);
