using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.Services;

/// <summary>
/// Usuario de demostracion de cada perfil, para rellenar la pantalla de login.
/// Coinciden con los usuarios que crea HotelSystem.Persistence/Semilla/SembradorDatos.cs.
/// Son solo una ayuda visual: la validacion real la hace el backend contra la base de datos.
/// </summary>
public static class CredencialesDemo
{
    private static readonly Dictionary<Role, (string Username, string Password, string Display)> Datos = new()
    {
        [Role.Huesped] = ("valentina.lagos", "guest2026", "Valentina Lagos"),
        [Role.Recepcion] = ("mlopez", "recep2026", "Maria Lopez"),
        [Role.Housekeeping] = ("amartinez", "hk2026", "Ana Martinez"),
        [Role.Coordinador] = ("ltorres", "coord2026", "Luis Torres"),
        [Role.Servicios] = ("pgomez", "serv2026", "Paula Gomez"),
        [Role.Admin] = ("rsilva", "admin2026", "Roberto Silva"),
    };

    public static (string Username, string Password, string Display) De(Role role) => Datos[role];
}
