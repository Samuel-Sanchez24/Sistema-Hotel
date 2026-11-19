using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.CierreCuenta;
using HotelSystem.Application.Usuarios;
using HotelSystem.Web.DTOs.Admin;
using HotelSystem.Web.DTOs.Common;
using HotelSystem.Web.Filters;
using HotelSystem.Web.Helpers;
using HotelSystem.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace HotelSystem.Web.Controllers;

[RoleAuthorize(Role.Admin)]
public class AdminController(
    IUsuarioService usuarios,
    IReporteService reportes,
    IReloj reloj) : HotelControllerBase
{
    private static readonly Role[] AssignableRoles =
        [Role.Recepcion, Role.Housekeeping, Role.Coordinador, Role.Servicios, Role.Admin];

    /* ─── HU-01: Gestion de usuarios ─── */

    public IActionResult Index(bool nuevo = false, int? editar = null)
    {
        var model = BuildUsers();
        model.ShowForm = nuevo;
        model.EditId = editar;
        model.CreatedInfo = TempData["NuevoUsuario"] as string;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Crear(CreateStaffUserDto form)
    {
        var rol = form.Role.ADominio();
        if (string.IsNullOrWhiteSpace(form.Name) || string.IsNullOrWhiteSpace(form.Email) || rol is null)
        {
            return ConErrorDeFormulario(form, "Complete nombre y correo.");
        }

        // Factory Method en el backend: crea el Empleado con un unico rol (RN-08), genera su
        // usuario y una contrasena temporal, y guarda solo el hash.
        var nombre = form.Name.Trim();
        var correo = form.Email.Trim();
        var rolEmpleado = rol.Value;
        EmpleadoCreadoDto? creado = null;
        var error = Intentar(() => creado = usuarios.CrearEmpleado(nombre, correo, rolEmpleado, UsuarioId));
        if (error is not null || creado is null)
        {
            return ConErrorDeFormulario(form, error ?? "No fue posible crear el usuario.");
        }

        Toast($"Usuario creado: {creado.Empleado.Nombre} ({Ui.RoleLabel(form.Role)})");
        TempData["NuevoUsuario"] = $"Cuenta creada para {creado.Empleado.Nombre}. Usuario: {creado.Empleado.Usuario} - Contrasena temporal: {creado.ContrasenaTemporal}";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult ToggleActivo(int id)
    {
        EmpleadoDto? empleado = null;
        var error = Intentar(() => empleado = usuarios.AlternarEstado(id, UsuarioId));

        if (error is not null || empleado is null)
        {
            Toast(error ?? "No fue posible actualizar la cuenta.", ToastType.Error);
        }
        else
        {
            Toast(empleado.Activo
                ? $"Cuenta reactivada: {empleado.Nombre} - rol {Ui.RoleLabel(empleado.Rol.AWeb())} restaurado (HU-01 Esc.5)"
                : $"Cuenta desactivada: {empleado.Nombre} (HU-01 Esc.3)");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult CambiarRol(ChangeRoleDto dto)
    {
        var rol = dto.Role.ADominio();
        if (rol is null) return RedirectToAction(nameof(Index));

        var rolEmpleado = rol.Value;
        var error = Intentar(() => usuarios.CambiarRol(dto.UserId, rolEmpleado, UsuarioId));

        if (error is not null) Toast(error, ToastType.Error);
        else Toast("Rol actualizado. Se retiraron los permisos anteriores (RN-08 - HU-01 Esc.4).");

        return RedirectToAction(nameof(Index));
    }

    /* ─── HU-13: Reportes de gestion ─── */

    public IActionResult Reportes(ReportFilterDto filtro)
    {
        var (desde, hasta) = Periodo(filtro);

        // El backend calcula el reporte con los datos reales del rango de fechas.
        ReporteGestion? reporte = null;
        var error = Intentar(() => reporte = reportes.Generar(desde, hasta));

        var model = new ReportsPageDto { Filter = filtro, Error = error };
        if (reporte is null) return View(model);

        filtro.NoData = reporte.SinDatos;
        model.PeriodLabel = reporte.Periodo;
        model.OccupancyRate = reporte.TasaOcupacion;
        model.OccupiedNights = reporte.NochesOcupadas;
        model.AvailableNights = reporte.NochesDisponibles;
        model.TotalRooms = reporte.TotalHabitaciones;
        model.OccupiedNow = reporte.HabitacionesOcupadasHoy;
        model.AvailableNow = reporte.HabitacionesDisponiblesHoy;
        model.Occupancy = reporte.Ocupacion
            .Select(o => new OccupancyRowDto(Ui.TypeLabel(o.Tipo.AWeb()), o.NochesDisponibles, o.NochesOcupadas))
            .ToList();
        model.Income = reporte.Ingresos.Select(i => new IncomeRowDto(i.Categoria, i.Monto)).ToList();
        model.Services = reporte.Servicios.Select(s => new ServiceReportRowDto(s.Servicio, s.Consumos, s.Total)).ToList();
        model.StaffRows = reporte.Personal
            .Select(p => new StaffReportRowDto(p.Nombre, p.Rol.AWeb(), p.Activo, p.Operaciones))
            .ToList();
        return View(model);
    }

    /// <summary>Devuelve el reporte del periodo como archivo PDF o Excel (HU-13 Esc.1, RF-13).</summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Exportar(ReportFilterDto filtro)
    {
        var (desde, hasta) = Periodo(filtro);

        // Strategy en el backend: el formato elegido decide que exportador arma el archivo.
        var formato = filtro.Format == "Excel" ? FormatoReporte.Excel : FormatoReporte.Pdf;
        ArchivoExportado? archivo = null;
        var error = Intentar(() => archivo = reportes.Exportar(desde, hasta, formato, UsuarioId));

        if (error is not null || archivo is null)
        {
            Toast(error ?? "No fue posible exportar el reporte.", ToastType.Error);
            return RedirectToAction(nameof(Reportes), new { filtro.Tab, filtro.DateFrom, filtro.DateTo });
        }

        return File(archivo.Contenido, archivo.TipoContenido, archivo.NombreArchivo);
    }

    /* ─── Auxiliares ─── */

    private AdminUsersPageDto BuildUsers() => new()
    {
        Staff = usuarios.ListarPersonal().Select(e => e.AWeb()).ToList(),
        AssignableRoles = AssignableRoles,
    };

    private IActionResult ConErrorDeFormulario(CreateStaffUserDto form, string mensaje)
    {
        var model = BuildUsers();
        model.ShowForm = true;
        model.Form = form;
        model.FormError = mensaje;
        return View(nameof(Index), model);
    }

    /// <summary>
    /// Rango de fechas del filtro. Por defecto, del primer dia del mes hasta hoy.
    /// Deja las fechas normalizadas en el filtro para que los campos las muestren.
    /// </summary>
    private (DateOnly Desde, DateOnly Hasta) Periodo(ReportFilterDto filtro)
    {
        var hoy = reloj.Hoy;
        var desde = Ui.ParseDate(filtro.DateFrom) ?? new DateOnly(hoy.Year, hoy.Month, 1);
        var hasta = Ui.ParseDate(filtro.DateTo) ?? hoy;

        filtro.DateFrom = Ui.IsoDate(desde);
        filtro.DateTo = Ui.IsoDate(hasta);
        return (desde, hasta);
    }
}
