using HotelSystem.Application;
using HotelSystem.Application.Comun;
using HotelSystem.Infrastructure;
using HotelSystem.Infrastructure.Correo;
using HotelSystem.Infrastructure.Reloj;
using HotelSystem.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

/* ─── Backend: configuracion (appsettings.json) y registro de las capas ─── */

var configuracion = builder.Configuration;

var baseDeDatos = configuracion.GetSection("BaseDeDatos").Get<OpcionesBaseDeDatos>() ?? new OpcionesBaseDeDatos();
if (args.Contains("--reiniciar-datos")) baseDeDatos.ReiniciarAlIniciar = true;

var correo = configuracion.GetSection("Correo").Get<OpcionesCorreo>() ?? new OpcionesCorreo();
if (string.IsNullOrWhiteSpace(correo.CarpetaSalida))
    correo.CarpetaSalida = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "correos");

var hotel = OpcionesHotel.Desde(
    configuracion["Hotel:HoraCheckIn"],
    configuracion["Hotel:HorasCancelacionSinCargo"],
    configuracion["Hotel:HorasCancelacionTour"]);
var reloj = OpcionesReloj.Desde(configuracion["Hotel:FechaActual"]);

builder.Services.AddAplicacion(hotel);                // servicios por modulo, reglas de negocio, patrones
builder.Services.AddPersistencia(baseDeDatos);        // Entity Framework Core y repositorios
builder.Services.AddInfraestructura(correo, reloj);   // correo, reloj, contrasenas, exportadores

var app = builder.Build();

// Crea la base de datos y carga los datos de demostracion la primera vez.
var activa = app.Services.InicializarBaseDeDatos();
app.Logger.LogInformation("Base de datos en uso: {Descripcion}", activa.Descripcion);
app.Logger.LogInformation("Correo en modo {Modo}. Carpeta de salida: {Carpeta}", correo.Modo, correo.CarpetaSalida);

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();
app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
   .WithStaticAssets();

app.Run();
