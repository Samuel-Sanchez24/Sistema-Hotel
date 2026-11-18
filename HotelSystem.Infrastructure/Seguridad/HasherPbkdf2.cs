using System.Globalization;
using System.Security.Cryptography;
using HotelSystem.Application.Abstracciones;

namespace HotelSystem.Infrastructure.Seguridad;

/// <summary>
/// Hash de contraseñas con PBKDF2-SHA256 y una sal aleatoria por usuario.
/// El texto guardado tiene la forma  PBKDF2-SHA256$iteraciones$sal$hash  (sal y hash en Base64),
/// de modo que la verificación sabe con qué parámetros se generó.
/// </summary>
public sealed class HasherPbkdf2 : IHasherContrasenas
{
    private const string Algoritmo = "PBKDF2-SHA256";
    private const int Iteraciones = 100_000;
    private const int BytesSal = 16;
    private const int BytesHash = 32;

    public string Hash(string contrasena)
    {
        var sal = RandomNumberGenerator.GetBytes(BytesSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, BytesHash);

        return string.Join('$',
            Algoritmo,
            Iteraciones.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(sal),
            Convert.ToBase64String(hash));
    }

    public bool Verificar(string contrasena, string hash)
    {
        if (string.IsNullOrEmpty(contrasena) || string.IsNullOrEmpty(hash)) return false;

        var partes = hash.Split('$');
        if (partes.Length != 4 || partes[0] != Algoritmo) return false;
        if (!int.TryParse(partes[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iteraciones) || iteraciones < 1) return false;

        try
        {
            var sal = Convert.FromBase64String(partes[2]);
            var esperado = Convert.FromBase64String(partes[3]);
            var calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

            // Comparación en tiempo constante: no revela cuántos bytes coinciden.
            return CryptographicOperations.FixedTimeEquals(calculado, esperado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
