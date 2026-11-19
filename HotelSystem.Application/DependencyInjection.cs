using HotelSystem.Application.CierreCuenta;
using HotelSystem.Application.Comun;
using HotelSystem.Application.Housekeeping;
using HotelSystem.Application.Reservas;
using HotelSystem.Application.ServiciosAdicionales;
using HotelSystem.Application.Tours;
using HotelSystem.Application.Usuarios;
using HotelSystem.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace HotelSystem.Application;

/// <summary>Registro de los servicios de la capa de aplicación. Lo llaman HotelSystem.Web y HotelSystem.API.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddAplicacion(this IServiceCollection services, OpcionesHotel? opciones = null)
    {
        services.AddSingleton(opciones ?? new OpcionesHotel());
        services.AddScoped<Bitacora>();

        // Observer: los dos paneles son únicos para toda la aplicación y se registran
        // también como observadores. Un tercer observador se agregaría aquí, sin tocar los servicios.
        services.AddSingleton<PanelRecepcion>();
        services.AddSingleton<PanelHousekeeping>();
        services.AddSingleton<IObservadorHabitacion>(sp => sp.GetRequiredService<PanelRecepcion>());
        services.AddSingleton<IObservadorHabitacion>(sp => sp.GetRequiredService<PanelHousekeeping>());
        services.AddSingleton<SuscriptorHabitaciones>();

        // Política de cancelación (RN-09).
        services.AddSingleton<IPoliticaCancelacion, PoliticaCancelacion48Horas>();

        // Factory Method: creadores de usuarios.
        services.AddScoped<CreadorHuesped>();
        services.AddScoped<CreadorEmpleado>();

        // Un servicio por módulo (los seis componentes del C4 nivel 3).
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        services.AddScoped<IHabitacionService, HabitacionService>();
        services.AddScoped<IReservaService, ReservaService>();
        services.AddScoped<ITourService, TourService>();
        services.AddScoped<IServicioAdicionalService, ServicioAdicionalService>();
        services.AddScoped<ICuentaService, CuentaFacade>();       // Facade
        services.AddScoped<IReporteService, GeneradorReportes>(); // contexto del Strategy

        return services;
    }
}
