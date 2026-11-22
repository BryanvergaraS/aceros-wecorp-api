using System.Net;
using System.Net.Http.Json;
using WeCorp.Ventas.Api.Dtos;

namespace WeCorp.Ventas.Api.Tests;

/// <summary>CRUD de clientes (RF06, HU07, HU08).</summary>
public class ClientesTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Crear_ConRutValido_Devuelve201_YNormalizaElRut()
    {
        var vendedor = factory.ClienteVendedor();

        var respuesta = await vendedor.PostAsJsonAsync("/api/v1/clientes",
            new { rut = "12.345.678-5", nombre = "Ferretería de prueba" });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        Assert.NotNull(respuesta.Headers.Location);
        var cliente = await respuesta.Content.ReadFromJsonAsync<ClienteDto>();
        Assert.Equal("12345678-5", cliente!.Rut);
    }

    [Fact]
    public async Task Crear_ConDigitoVerificadorIncorrecto_Devuelve400()
    {
        var respuesta = await factory.ClienteVendedor().PostAsJsonAsync("/api/v1/clientes",
            new { rut = "12345678-9", nombre = "RUT inválido" });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_ConRutRepetido_Devuelve409()
    {
        // 11.111.111-1 es un cliente de los datos semilla.
        var respuesta = await factory.ClienteVendedor().PostAsJsonAsync("/api/v1/clientes",
            new { rut = "11.111.111-1", nombre = "Duplicado" });

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_ComoVendedor_Devuelve403()
    {
        var respuesta = await factory.ClienteVendedor().DeleteAsync("/api/v1/clientes/1");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_ClienteConNotas_Devuelve409()
    {
        var crear = await factory.ClienteVendedor().PostAsJsonAsync("/api/v1/notas-venta", new
        {
            clienteId = 2,
            lineas = new[] { new { productoId = FakeCatalogoClient.ProductoBarra, cantidad = 1 } }
        });
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);

        var respuesta = await factory.ClienteAdmin().DeleteAsync("/api/v1/clientes/2");

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Eliminar_ClienteSinNotas_Devuelve204()
    {
        var admin = factory.ClienteAdmin();
        var crear = await admin.PostAsJsonAsync("/api/v1/clientes", new { rut = "7.654.321-6", nombre = "Para eliminar" });
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);
        var cliente = await crear.Content.ReadFromJsonAsync<ClienteDto>();

        var respuesta = await admin.DeleteAsync($"/api/v1/clientes/{cliente!.Id}");

        Assert.Equal(HttpStatusCode.NoContent, respuesta.StatusCode);
    }
}
