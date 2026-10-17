using HotelSystem.Web.DTOs.Common;

namespace HotelSystem.Web.Helpers;

public record NavItem(string Label, string Icon, string Controller, string Action);

/// <summary>Menu lateral por rol (equivalente a NAV del prototipo).</summary>
public static class Navigation
{
    private static readonly Dictionary<Role, NavItem[]> Items = new()
    {
        [Role.Huesped] =
        [
            new("Mis Reservas", Icons.Home, "Huesped", "Index"),
            new("Reservar Habitacion", Icons.Calendar, "Huesped", "ReservarHabitacion"),
            new("Reservar Tour", Icons.Map, "Huesped", "ReservarTour"),
            new("Solicitar Servicio", Icons.Plus, "Huesped", "SolicitarServicio"),
            new("Mis Consumos", Icons.Clipboard, "Huesped", "Consumos"),
            new("Mi Cuenta", Icons.Card, "Huesped", "MiCuenta"),
        ],
        [Role.Recepcion] =
        [
            new("Panel Habitaciones", Icons.Rooms, "Recepcion", "Index"),
            new("Check-in / Check-out", Icons.Check, "Recepcion", "CheckInOut"),
            new("Facturacion y Pago", Icons.Card, "Recepcion", "Facturacion"),
        ],
        [Role.Housekeeping] =
        [
            new("Estado Habitaciones", Icons.Rooms, "Housekeeping", "Index"),
            new("Plan de Limpieza", Icons.ClipboardList, "Housekeeping", "PlanLimpieza"),
        ],
        [Role.Coordinador] =
        [
            new("Panel de Tours", Icons.Map, "Tours", "Index"),
            new("Asignacion Recursos", Icons.Users, "Tours", "Asignacion"),
        ],
        [Role.Servicios] =
        [
            new("Registrar Consumo", Icons.Plus, "Servicios", "Index"),
            new("Historial de Consumos", Icons.Clipboard, "Servicios", "Historial"),
        ],
        [Role.Admin] =
        [
            new("Gestion de Usuarios", Icons.UserGroup, "Admin", "Index"),
            new("Reportes de Gestion", Icons.Chart, "Admin", "Reportes"),
        ],
    };

    public static IReadOnlyList<NavItem> For(Role role) => Items[role];

    public static NavItem Home(Role role) => Items[role][0];
}
