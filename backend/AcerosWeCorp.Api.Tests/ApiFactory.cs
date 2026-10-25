using System.Net.Http.Headers;
using System.Net.Http.Json;
using AcerosWeCorp.Api.Data;
using AcerosWeCorp.Api.Dtos;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AcerosWeCorp.Api.Tests;

/// <summary>
/// Levanta la API completa en memoria (middleware, JWT, roles, controladores) y cambia
/// SQL Server por una base en memoria para que las pruebas no dependan de un servidor.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    public const string AdminEmail = "admin@test.cl";
    public const string AdminPassword = "AdminPrueba123";
    public const string VendedorEmail = "vendedor@test.cl";
    public const string VendedorPassword = "VendedorPrueba123";

    private readonly string _nombreBase = $"AcerosWeCorpTest-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseSetting("Jwt:Key", "clave-solo-para-pruebas-de-integracion-0123456789");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "no-se-usa-en-pruebas");
        builder.UseSetting("Seed:AdminEmail", AdminEmail);
        builder.UseSetting("Seed:AdminPassword", AdminPassword);
        builder.UseSetting("Seed:VendedorEmail", VendedorEmail);
        builder.UseSetting("Seed:VendedorPassword", VendedorPassword);

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<AppDbContext>>();
            services.AddDbContext<AppDbContext>(o => o.UseInMemoryDatabase(_nombreBase));
        });
    }

    /// <summary>Crea un cliente HTTP ya autenticado con el usuario indicado.</summary>
    public async Task<HttpClient> ClienteConLoginAsync(string email, string password)
    {
        var cliente = CreateClient();
        var respuesta = await cliente.PostAsJsonAsync("/api/v1/auth/login", new { email, password });
        respuesta.EnsureSuccessStatusCode();
        var login = await respuesta.Content.ReadFromJsonAsync<LoginResponse>();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", login!.Token);
        return cliente;
    }

    public Task<HttpClient> ClienteAdminAsync() => ClienteConLoginAsync(AdminEmail, AdminPassword);
    public Task<HttpClient> ClienteVendedorAsync() => ClienteConLoginAsync(VendedorEmail, VendedorPassword);
}
