using HotelSystem.Application.Abstracciones;
using HotelSystem.Application.CierreCuenta;
using HotelSystem.Infrastructure.Correo;
using HotelSystem.Infrastructure.Reloj;
using HotelSystem.Infrastructure.Reportes;
using HotelSystem.Infrastructure.Seguridad;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSystem.Infrastructure;

/// <summary>
/// Registro de los servicios técnicos: reloj, hash de contraseñas, correo y exportadores.
/// Lo llaman HotelSystem.Web y HotelSystem.API.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfraestructura(this IServiceCollection services, OpcionesCorreo? correo = null, OpcionesReloj? reloj = null)
    {
        services.AddSingleton(reloj ?? new OpcionesReloj());
        services.AddSingleton<IReloj, RelojSistema>();

        services.AddSingleton<IHasherContrasenas, HasherPbkdf2>();

        services.AddSingleton(correo ?? new OpcionesCorreo());
        services.AddSingleton<IServicioCorreo, ServicioCorreo>();

        // Strategy: cada formato de exportación es una estrategia. GeneradorReportes las recibe
        // todas y elige según el formato pedido. Un formato nuevo solo necesita una línea aquí.
        services.AddSingleton<IExportadorReporte, ExportadorPDF>();
        services.AddSingleton<IExportadorReporte, ExportadorExcel>();

        return services;
    }
}
