using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace WeCorp.Ventas.Api.Tests;

/// <summary>
/// Genera JWT iguales a los que emite la API de Catálogo (misma llave, emisor y audiencia),
/// para probar la API de Ventas sin levantar Catálogo.
/// </summary>
public static class TokenPruebas
{
    public const string Llave = "clave-solo-para-pruebas-de-integracion-0123456789";
    public const string Emisor = "WeCorp.Catalogo";
    public const string Audiencia = "WeCorp.Sistema";

    public const int AdminId = 1;
    public const int VendedorId = 2;
    public const int OtroVendedorId = 3;

    public static string Admin() => Crear(AdminId, "Admin Pruebas", "Administrador");
    public static string Vendedor() => Crear(VendedorId, "Vendedor Pruebas", "Vendedor");
    public static string OtroVendedor() => Crear(OtroVendedorId, "Otro Vendedor", "Vendedor");

    /// <summary>Token de vendedor que expiró hace 10 minutos.</summary>
    public static string Vencido() => Crear(VendedorId, "Vendedor Pruebas", "Vendedor", DateTime.UtcNow.AddMinutes(-10));

    /// <summary>Token firmado con otra llave (firma inválida).</summary>
    public static string ConOtraLlave() =>
        Crear(VendedorId, "Vendedor Pruebas", "Vendedor", llave: "otra-llave-distinta-para-pruebas-0123456789");

    public static string Crear(int id, string nombre, string rol, DateTime? expira = null, string llave = Llave)
    {
        var exp = expira ?? DateTime.UtcNow.AddMinutes(30);
        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, id.ToString()),
                new Claim(ClaimTypes.Name, nombre),
                new Claim(ClaimTypes.Email, $"usuario{id}@test.cl"),
                new Claim(ClaimTypes.Role, rol)
            }),
            IssuedAt = exp.AddMinutes(-60),
            NotBefore = exp.AddMinutes(-60),
            Expires = exp,
            Issuer = Emisor,
            Audience = Audiencia,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(llave)), SecurityAlgorithms.HmacSha256)
        };
        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
