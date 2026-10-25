using System.Security.Claims;
using System.Text;
using AcerosWeCorp.Api.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace AcerosWeCorp.Api.Services;

public interface ITokenService
{
    (string Token, DateTime Expira) Generar(Usuario usuario, string rol);
}

/// <summary>Genera el JWT. No guarda estado, por eso se registra como Singleton.</summary>
public class TokenService(IOptions<JwtOptions> opciones) : ITokenService
{
    private readonly JwtOptions _jwt = opciones.Value;
    private readonly JsonWebTokenHandler _handler = new();

    public (string Token, DateTime Expira) Generar(Usuario usuario, string rol)
    {
        var expira = DateTime.UtcNow.AddMinutes(_jwt.ExpiraMinutos);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Apellido}"),
            new(ClaimTypes.Email, usuario.Email),
            new(ClaimTypes.Role, rol)
        };

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expira,
            Issuer = _jwt.Issuer,
            Audience = _jwt.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key)), SecurityAlgorithms.HmacSha256)
        };

        return (_handler.CreateToken(descriptor), expira);
    }
}
