using System.Globalization;
using System.Net;
using HotelSystem.Web.DTOs.Common;
using Microsoft.AspNetCore.Html;

namespace HotelSystem.Web.Helpers;

/// <summary>
/// Equivalente en Razor de los componentes pequenos del prototipo (Ic, Badge, Pill)
/// y de los diccionarios de etiquetas y colores.
/// </summary>
public static class Ui
{
    private static readonly CultureInfo Co = CultureInfo.GetCultureInfo("es-CO");

    /// <summary>Fuente monoespaciada usada para numeros y montos.</summary>
    public const string Mono = "font-family:JetBrains Mono,monospace";

    /// <summary>Formato de moneda del prototipo: $120.000</summary>
    public static string Money(decimal n) => "$" + n.ToString("N0", Co);

    public static string Now() => DateTime.Now.ToString("HH:mm", Co);

    public static string Initials(string name) =>
        string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => w[0]));

    public static IHtmlContent Ic(string d, int s = 18) => new HtmlString(
        $"<svg width=\"{s}\" height=\"{s}\" viewBox=\"0 0 24 24\" fill=\"none\" stroke=\"currentColor\" stroke-width=\"1.75\" stroke-linecap=\"round\" stroke-linejoin=\"round\"><path d=\"{d}\"/></svg>");

    public static string TypeLabel(RoomType t) => t switch
    {
        RoomType.Simple => "Simple",
        RoomType.Doble => "Doble",
        _ => "Suite",
    };

    public static string RoleLabel(Role r) => r switch
    {
        Role.Huesped => "Huesped",
        Role.Recepcion => "Recepcion",
        Role.Housekeeping => "Housekeeping",
        Role.Coordinador => "Coord. de Tours",
        Role.Servicios => "Servicios",
        _ => "Administrador",
    };

    public static string StatusLabel(ReservationStatus s) => s switch
    {
        ReservationStatus.Confirmada => "Confirmada",
        ReservationStatus.EnCurso => "En curso",
        _ => "Cancelada",
    };

    public static string ReservationStatusColor(ReservationStatus s) => s switch
    {
        ReservationStatus.Confirmada => "bg-blue-500/10 text-blue-400 border-blue-500/20",
        ReservationStatus.EnCurso => "bg-emerald-500/10 text-emerald-400 border-emerald-500/20",
        _ => "bg-slate-600/10 text-slate-500 border-slate-600/20",
    };

    public record StatusStyle(string Label, string Dot, string Badge, string Text, string Ring);

    public static StatusStyle Sc(RoomStatus s) => s switch
    {
        RoomStatus.Disponible => new("Disponible", "bg-emerald-400", "bg-emerald-400/10 border-emerald-500/25", "text-emerald-400", "border-emerald-500/30"),
        RoomStatus.Ocupada => new("Ocupada", "bg-rose-400", "bg-rose-400/10 border-rose-500/25", "text-rose-400", "border-rose-500/30"),
        RoomStatus.EnLimpieza => new("En limpieza", "bg-amber-400", "bg-amber-400/10 border-amber-500/25", "text-amber-400", "border-amber-500/30"),
        _ => new("Mantenimiento", "bg-slate-500", "bg-slate-600/10 border-slate-600/25", "text-slate-500", "border-slate-600/30"),
    };

    public static string RoleColor(Role r) => r switch
    {
        Role.Admin => "bg-purple-500/10 text-purple-400 border-purple-500/20",
        Role.Recepcion => "bg-blue-500/10 text-blue-400 border-blue-500/20",
        Role.Housekeeping => "bg-amber-500/10 text-amber-400 border-amber-500/20",
        Role.Coordinador => "bg-teal-500/10 text-teal-400 border-teal-500/20",
        Role.Servicios => "bg-rose-500/10 text-rose-400 border-rose-500/20",
        _ => "bg-slate-500/10 text-slate-400 border-slate-500/20",
    };

    /// <summary>Barra de progreso de cupos: rojo lleno, ambar &gt;70%, verde el resto.</summary>
    public static string QuotaBarColor(TourDto t) =>
        t.IsFull ? "bg-rose-500" : t.Percent > 70 ? "bg-amber-500" : "bg-emerald-500";

    public static IHtmlContent Badge(RoomStatus status)
    {
        var c = Sc(status);
        return new HtmlString(
            $"<span class=\"inline-flex items-center gap-1.5 px-2.5 py-0.5 rounded-full text-xs font-medium border {c.Badge} {c.Text}\"><span class=\"w-1.5 h-1.5 rounded-full {c.Dot}\"></span>{c.Label}</span>");
    }

    public static IHtmlContent Pill(string color, string text) => new HtmlString(
        $"<span class=\"inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium border {color}\">{WebUtility.HtmlEncode(text)}</span>");

    /// <summary>
    /// Apertura del modal del prototipo. Los modales se muestran desde el servidor
    /// (por parametro en la URL) y se cierran navegando a <paramref name="closeUrl"/>.
    /// </summary>
    public static IHtmlContent ModalOpen(string title, string closeUrl, bool wide = false)
    {
        var url = WebUtility.HtmlEncode(closeUrl);
        return new HtmlString(
            $"<div class=\"fixed inset-0 bg-black/70 backdrop-blur-sm flex items-center justify-center z-50 p-4\" data-modal data-close-url=\"{url}\">" +
            $"<div class=\"bg-[#0e1a2e] border border-[#1a2d48] rounded-2xl p-6 w-full {(wide ? "max-w-lg" : "max-w-sm")} shadow-2xl\">" +
            "<div class=\"flex justify-between items-center mb-5\">" +
            $"<span class=\"text-slate-100 font-semibold\">{WebUtility.HtmlEncode(title)}</span>" +
            $"<a href=\"{url}\" class=\"text-slate-600 hover:text-slate-300\">{Ic(Icons.Close)}</a></div>");
    }

    public static IHtmlContent ModalClose() => new HtmlString("</div></div>");
}
