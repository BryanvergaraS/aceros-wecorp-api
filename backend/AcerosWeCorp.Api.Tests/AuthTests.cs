using System.Net;
using System.Net.Http.Json;
using AcerosWeCorp.Api.Dtos;

namespace AcerosWeCorp.Api.Tests;

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
    }

    [Fact]
    public async Task Login_ConClaveIncorrecta_Devuelve401()
    {
        var cliente = factory.CreateClient();

        var respuesta = await cliente.PostAsJsonAsync("/api/v1/auth/login",
            new { email = ApiFactory.AdminEmail, password = "clave-equivocada" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
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
    public async Task EndpointDeAdmin_ConVendedor_Devuelve403()
    {
        var cliente = await factory.ClienteVendedorAsync();

        var respuesta = await cliente.GetAsync("/api/v1/usuarios");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }
}
