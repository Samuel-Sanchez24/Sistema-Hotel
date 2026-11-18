using System.Globalization;
using System.Security.Claims;
using System.Text;
using HotelSystem.Application.Usuarios;
using HotelSystem.Domain.Enumeraciones;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace HotelSystem.API.Seguridad;

/// <summary>Configuracion de los tokens. Se lee de la seccion "Jwt" de appsettings.json.</summary>
public sealed class OpcionesJwt
{
    /// <summary>Clave secreta con la que se firman los tokens. Minimo 32 caracteres (HMAC-SHA256).</summary>
    public string Clave { get; set; } = string.Empty;

    public string Emisor { get; set; } = "HotelSystem.API";

    public string Audiencia { get; set; } = "HotelSystem.Front";

    /// <summary>Horas que dura una sesion antes de pedir un nuevo inicio de sesion.</summary>
    public int HorasVigencia { get; set; } = 8;

    public SymmetricSecurityKey ClaveFirma()
    {
        if (Encoding.UTF8.GetByteCount(Clave ?? string.Empty) < 32)
            throw new InvalidOperationException("La clave Jwt:Clave de appsettings.json debe tener al menos 32 caracteres.");

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Clave!));
    }
}

/// <summary>Nombres de los datos (claims) que viajan dentro del token.</summary>
public static class ClaimsToken
{
    public const string Id = "sub";
    public const string Nombre = "name";
    public const string Usuario = "preferred_username";
    public const string Rol = "role";
}

/// <summary>
/// Roles tal como viajan en el token y en las respuestas JSON (mismo formato que los enums:
/// MAYUSCULAS_CON_GUION_BAJO). Cada usuario tiene un unico rol (RN-08).
/// </summary>
public static class RolesApi
{
    public const string Huesped = "HUESPED";
    public const string Recepcion = "RECEPCION";
    public const string Housekeeping = "HOUSEKEEPING";
    public const string Coordinador = "COORDINADOR_TOURS";
    public const string Servicios = "SERVICIOS";
    public const string Administrador = "ADMINISTRADOR";

    public static string De(UsuarioAutenticadoDto usuario) => usuario.RolPersonal switch
    {
        null => Huesped,
        RolEmpleado.Recepcion => Recepcion,
        RolEmpleado.Housekeeping => Housekeeping,
        RolEmpleado.CoordinadorTours => Coordinador,
        RolEmpleado.Servicios => Servicios,
        _ => Administrador,
    };
}

public sealed record TokenEmitido(string Token, DateTime ExpiraUtc);

/// <summary>Crea el token JWT que el front envia en cada peticion (Authorization: Bearer).</summary>
public sealed class GeneradorTokens(OpcionesJwt opciones)
{
    public TokenEmitido Emitir(UsuarioAutenticadoDto usuario)
    {
        // Se usa la hora real (UTC) y no IReloj: la fecha de demostracion no debe afectar la vigencia del token.
        var ahora = DateTime.UtcNow;
        var expira = ahora.AddHours(opciones.HorasVigencia);

        var claims = new Claim[]
        {
            new(ClaimsToken.Id, usuario.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimsToken.Nombre, usuario.Nombre),
            new(ClaimsToken.Usuario, usuario.Usuario),
            new(ClaimsToken.Rol, RolesApi.De(usuario)),
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = opciones.Emisor,
            Audience = opciones.Audiencia,
            IssuedAt = ahora,
            NotBefore = ahora,
            Expires = expira,
            SigningCredentials = new SigningCredentials(opciones.ClaveFirma(), SecurityAlgorithms.HmacSha256),
        };

        return new TokenEmitido(new JsonWebTokenHandler().CreateToken(descriptor), expira);
    }
}
