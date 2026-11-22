using System.Net;

namespace WeCorp.Ventas.Api.Tests;

/// <summary>Validación del JWT emitido por Catálogo (RNF02, RNF03) y health check (RNF07).</summary>
public class SeguridadTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task SinToken_Devuelve401()
    {
        var respuesta = await factory.CreateClient().GetAsync("/api/v1/clientes");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task TokenVencido_Devuelve401()
    {
        var respuesta = await factory.ClienteCon(TokenPruebas.Vencido()).GetAsync("/api/v1/clientes");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task TokenConFirmaInvalida_Devuelve401()
    {
        var respuesta = await factory.ClienteCon(TokenPruebas.ConOtraLlave()).GetAsync("/api/v1/clientes");

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task TokenValido_Devuelve200()
    {
        var respuesta = await factory.ClienteVendedor().GetAsync("/api/v1/clientes");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task Health_SinToken_Devuelve200()
    {
        var respuesta = await factory.CreateClient().GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }
}
