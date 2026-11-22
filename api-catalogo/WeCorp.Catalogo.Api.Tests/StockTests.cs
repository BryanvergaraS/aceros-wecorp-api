using System.Net;
using System.Net.Http.Json;
using WeCorp.Catalogo.Api.Dtos;

namespace WeCorp.Catalogo.Api.Tests;

/// <summary>Endpoints de stock que consume la API de Ventas (RF08, RF11).</summary>
public class StockTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    /// <summary>Crea un producto propio de la prueba para no depender de los datos semilla.</summary>
    private async Task<ProductoDto> CrearProductoAsync(HttpClient admin, string sku, int stock, bool activo = true)
    {
        var respuesta = await admin.PostAsJsonAsync("/api/v1/productos", new
        {
            familiaId = 1, sku, nombre = $"Producto {sku}", unidadVenta = "Tira",
            precioVenta = 5000, stock, stockMinimo = 0, activo
        });
        respuesta.EnsureSuccessStatusCode();
        return (await respuesta.Content.ReadFromJsonAsync<ProductoDto>())!;
    }

    [Fact]
    public async Task Descontar_ComoVendedor_DescuentaYDevuelvePrecio()
    {
        var admin = await factory.ClienteAdminAsync();
        var producto = await CrearProductoAsync(admin, "STK-DESC", 10);
        var vendedor = await factory.ClienteVendedorAsync();

        // Dos líneas del mismo producto se suman: 3 + 2 = 5.
        var respuesta = await vendedor.PostAsJsonAsync("/api/v1/productos/stock/descontar", new
        {
            lineas = new[] { new { productoId = producto.Id, cantidad = 3 }, new { productoId = producto.Id, cantidad = 2 } }
        });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var resultado = await respuesta.Content.ReadFromJsonAsync<MovimientoStockResultadoDto>();
        var linea = Assert.Single(resultado!.Lineas);
        Assert.Equal(5000, linea.PrecioUnitario);
        Assert.Equal(5, linea.StockRestante);
    }

    [Fact]
    public async Task Descontar_ConStockInsuficiente_Devuelve422_YNoDescuentaNada()
    {
        var admin = await factory.ClienteAdminAsync();
        var conStock = await CrearProductoAsync(admin, "STK-OK", 10);
        var sinStock = await CrearProductoAsync(admin, "STK-POCO", 1);

        var respuesta = await admin.PostAsJsonAsync("/api/v1/productos/stock/descontar", new
        {
            lineas = new[] { new { productoId = conStock.Id, cantidad = 4 }, new { productoId = sinStock.Id, cantidad = 2 } }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var despues = await admin.GetFromJsonAsync<ProductoDto>($"/api/v1/productos/{conStock.Id}");
        Assert.Equal(10, despues!.Stock); // todo o nada
    }

    [Fact]
    public async Task Descontar_ProductoInactivo_Devuelve422()
    {
        var admin = await factory.ClienteAdminAsync();
        var inactivo = await CrearProductoAsync(admin, "STK-INACT", 10, activo: false);

        var respuesta = await admin.PostAsJsonAsync("/api/v1/productos/stock/descontar", new
        {
            lineas = new[] { new { productoId = inactivo.Id, cantidad = 1 } }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    [Fact]
    public async Task Descontar_ProductoInexistente_Devuelve404()
    {
        var admin = await factory.ClienteAdminAsync();

        var respuesta = await admin.PostAsJsonAsync("/api/v1/productos/stock/descontar", new
        {
            lineas = new[] { new { productoId = 999999, cantidad = 1 } }
        });

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Descontar_SinLineas_Devuelve400()
    {
        var admin = await factory.ClienteAdminAsync();

        var respuesta = await admin.PostAsJsonAsync("/api/v1/productos/stock/descontar", new { lineas = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Reponer_SumaStock()
    {
        var admin = await factory.ClienteAdminAsync();
        var producto = await CrearProductoAsync(admin, "STK-REP", 2);

        var respuesta = await admin.PostAsJsonAsync("/api/v1/productos/stock/reponer", new
        {
            lineas = new[] { new { productoId = producto.Id, cantidad = 3 } }
        });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var despues = await admin.GetFromJsonAsync<ProductoDto>($"/api/v1/productos/{producto.Id}");
        Assert.Equal(5, despues!.Stock);
    }
}
