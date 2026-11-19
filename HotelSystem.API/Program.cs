using System.Text.Json;
using System.Text.Json.Serialization;
using HotelSystem.API.Contratos;
using HotelSystem.API.Infraestructura;
using HotelSystem.API.Seguridad;
using HotelSystem.Application;
using HotelSystem.Application.Comun;
using HotelSystem.Infrastructure;
using HotelSystem.Infrastructure.Correo;
using HotelSystem.Infrastructure.Reloj;
using HotelSystem.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);
var configuracion = builder.Configuration;

/* ─── Backend: las mismas capas que usa la web MVC (Application, Persistence, Infrastructure) ─── */

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

builder.Services.AddAplicacion(hotel);
builder.Services.AddPersistencia(baseDeDatos);
builder.Services.AddInfraestructura(correo, reloj);

/* ─── REST: JSON, errores, autenticacion JWT y CORS para el front ─── */

builder.Services
    .AddControllers(opciones => opciones.Filters.Add<FiltroErroresNegocio>())
    .AddJsonOptions(opciones =>
    {
        // Enums como texto en MAYUSCULAS: "DISPONIBLE", "EN_LIMPIEZA", "COORDINADOR_TOURS".
        opciones.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.SnakeCaseUpper));
    })
    .ConfigureApiBehaviorOptions(opciones =>
    {
        // Un JSON mal formado tambien responde { "mensaje": ... } como el resto de errores.
        opciones.InvalidModelStateResponseFactory = contexto =>
        {
            var detalle = contexto.ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .FirstOrDefault(m => !string.IsNullOrWhiteSpace(m));
            return new BadRequestObjectResult(new RespuestaError(
                "Los datos enviados no son validos." + (detalle is null ? string.Empty : $" {detalle}")));
        };
    });

builder.Services.AddOpenApi();

var jwt = configuracion.GetSection("Jwt").Get<OpcionesJwt>() ?? new OpcionesJwt();
builder.Services.AddSingleton(jwt);
builder.Services.AddSingleton<GeneradorTokens>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        // Se conservan los nombres cortos de los claims ("sub", "role") tal como se emiten.
        opciones.MapInboundClaims = false;
        opciones.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Emisor,
            ValidateAudience = true,
            ValidAudience = jwt.Audiencia,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = jwt.ClaveFirma(),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1),
            NameClaimType = ClaimsToken.Nombre,
            RoleClaimType = ClaimsToken.Rol,
        };
        opciones.Events = new JwtBearerEvents
        {
            OnChallenge = async contexto =>
            {
                contexto.HandleResponse();
                contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await contexto.Response.WriteAsJsonAsync(new RespuestaError(
                    "Debe iniciar sesion. Envie el token en la cabecera Authorization: Bearer <token>."));
            },
            OnForbidden = contexto =>
            {
                contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
                return contexto.Response.WriteAsJsonAsync(new RespuestaError(
                    "Acceso denegado: su rol no tiene permiso para esta operacion (HU-02 Esc.4)."));
            },
        };
    });
builder.Services.AddAuthorization();

var origenes = configuracion.GetSection("Cors:Origenes").Get<string[]>()
    ?? ["http://127.0.0.1:5500", "http://localhost:5500"];
builder.Services.AddCors(opciones => opciones.AddPolicy("Front", politica => politica
    .WithOrigins(origenes)
    .AllowAnyHeader()
    .AllowAnyMethod()
    .WithExposedHeaders("Content-Disposition")));

var app = builder.Build();

// Crea la base de datos y carga los datos de demostracion la primera vez (la comparte con la web).
var activa = app.Services.InicializarBaseDeDatos();
app.Logger.LogInformation("Base de datos en uso: {Descripcion}", activa.Descripcion);

// Cualquier error no previsto responde 500 con el mismo formato { "mensaje": ... }.
app.UseExceptionHandler(errores => errores.Run(async contexto =>
{
    contexto.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await contexto.Response.WriteAsJsonAsync(new RespuestaError("Ocurrio un error inesperado en el servidor. Intente de nuevo."));
}));

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi(); // documento OpenAPI en /openapi/v1.json
}

app.UseHttpsRedirection();
app.UseCors("Front");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/", () => Results.Ok(new
{
    sistema = "Hotel Fast and Furious - API",
    estado = "en linea",
    baseDeDatos = activa.Descripcion,
    documentacion = "/openapi/v1.json",
}));

app.Run();
