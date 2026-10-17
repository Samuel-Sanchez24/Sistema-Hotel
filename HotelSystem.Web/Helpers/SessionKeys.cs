using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.Helpers;

public static class SessionKeys
{
    public const string Role = "Role";
    public const string UserName = "UserName";

    public static Role? GetRole(this ISession session) =>
        Enum.TryParse<Role>(session.GetString(Role), out var r) ? r : null;

    public static string GetUserName(this ISession session) => session.GetString(UserName) ?? "";
}
