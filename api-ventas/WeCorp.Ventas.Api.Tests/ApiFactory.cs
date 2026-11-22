using System.Net.Http.Headers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WeCorp.Ventas.Api.Data;
using WeCorp.Ventas.Api.Services;

namespace WeCorp.Ventas.Api.Tests;

/// <summary>
/// Levanta la API de Ventas completa en memoria (middleware, JWT, roles, controladores),
/// cambia SQL Server por una base en memoria y la API de Catálogo por <see cref="FakeCatalogoClient"/>.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    private readonly string _nombreBase = $"WeCorpVentasTest-{Guid.NewGuid()}";

    public FakeCatalogoClient Catalogo { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Key", TokenPruebas.Llave);
        builder.UseSetting("Jwt:Issuer", TokenPruebas.Emisor);
        builder.UseSetting("Jwt:Audience", TokenPruebas.Audiencia);
        builder.UseSetting("ConnectionStrings:VentasDb", "no-se-usa-en-pruebas");
        builder.UseSetting("Catalogo:BaseUrl", "http://catalogo.pruebas");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_nombreBase));

            services.RemoveAll<ICatalogoClient>();
            services.AddSingleton<ICatalogoClient>(Catalogo);
        });
    }

    public HttpClient ClienteCon(string token)
    {
        var cliente = CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return cliente;
    }

    public HttpClient ClienteAdmin() => ClienteCon(TokenPruebas.Admin());
    public HttpClient ClienteVendedor() => ClienteCon(TokenPruebas.Vendedor());
    public HttpClient ClienteOtroVendedor() => ClienteCon(TokenPruebas.OtroVendedor());
}
