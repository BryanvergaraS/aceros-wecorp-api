using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WeCorp.Catalogo.Api.Dtos;

namespace WeCorp.Catalogo.Api.Tests;

/// <summary>Login, JWT y control de acceso (RF01, RNF02, RNF03).</summary>
public class AuthTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveTokenYRol()
    {
        var cliente = factory.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/v1/auth/login",
            new { email = ApiFactory.AdminEmail, password = ApiFactory.AdminPassword });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var login = await respuesta.Content.ReadFromJsonAsync<LoginResponse>();
        Assert.False(string.IsNullOrWhiteSpace(login!.Token));
        Assert.Equal("Administrador", login.Usuario.Rol);
        Assert.True(login.Expira > DateTime.UtcNow.AddMinutes(55));
    }

    [Fact]
    public async Task Login_ConClaveIncorrecta_Devuelve401EnProblemDetails()
    {
        var cliente = factory.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/v1/auth/login",
            new { email = ApiFactory.AdminEmail, password = "clave-equivocada" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Login_SinEmail_Devuelve400()
    {
        var cliente = factory.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/v1/auth/login", new { email = "", password = "x" });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_SinToken_Devuelve401()
    {
        var cliente = factory.CreateClient();

        var respuesta = await cliente.GetAsync("/api/v1/productos");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_ConTokenVencido_Devuelve401()
    {
        var cliente = factory.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenVencido());

        var respuesta = await cliente.GetAsync("/api/v1/productos");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task EndpointDeAdmin_ConVendedor_Devuelve403()
    {
        var cliente = await factory.ClienteVendedorAsync();

        var respuesta = await cliente.GetAsync("/api/v1/usuarios");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Health_SinToken_Devuelve200()
    {
        var cliente = factory.CreateClient();

        var respuesta = await cliente.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    /// <summary>Token firmado con la llave correcta pero expirado hace 10 minutos.</summary>
    private static string TokenVencido() => new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
    {
        Subject = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
            new Claim(ClaimTypes.Role, "Administrador")
        }),
        NotBefore = DateTime.UtcNow.AddHours(-2),
        IssuedAt = DateTime.UtcNow.AddHours(-2),
        Expires = DateTime.UtcNow.AddMinutes(-10),
        Issuer = "WeCorp.Catalogo",
        Audience = "WeCorp.Sistema",
        SigningCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(ApiFactory.LlavePruebas)), SecurityAlgorithms.HmacSha256)
    });
}
