using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.Helpers;

public static class SessionKeys
{
    public const string Role = "Role";
    public const string UserName = "UserName";

    /// <summary>Identificador del usuario en la base de datos (huesped o empleado).</summary>
    public const string UserId = "UserId";

    public static Role? GetRole(this ISession session) =>
        Enum.TryParse<Role>(session.GetString(Role), out var r) ? r : null;

    public static string GetUserName(this ISession session) => session.GetString(UserName) ?? "";

    public static int? GetUserId(this ISession session) => session.GetInt32(UserId);
}
