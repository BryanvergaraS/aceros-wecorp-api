using System.Net;
using System.Net.Http.Json;
using WeCorp.Ventas.Api.Dtos;

namespace WeCorp.Ventas.Api.Tests;

/// <summary>Emisión y ciclo de vida de las notas de venta (RF07 a RF11, HU09 a HU11).</summary>
public class NotasVentaTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static object NuevaNota(int productoId, int cantidad, int clienteId = 1) => new
    {
        clienteId,
        lineas = new[] { new { productoId, cantidad } }
    };

    private async Task<NotaVentaDto> EmitirAsync(HttpClient cliente, int productoId, int cantidad)
    {
        var respuesta = await cliente.PostAsJsonAsync("/api/v1/notas-venta", NuevaNota(productoId, cantidad));
        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        return (await respuesta.Content.ReadFromJsonAsync<NotaVentaDto>())!;
    }

    [Fact]
    public async Task Emitir_Devuelve201_ConFolioTotalYPrecioDelCatalogo_YDescuentaStock()
    {
        var vendedor = factory.ClienteVendedor();
        var stockAntes = factory.Catalogo.Stock(FakeCatalogoClient.ProductoAngulo);

        var respuesta = await vendedor.PostAsJsonAsync("/api/v1/notas-venta", new
        {
            clienteId = 1,
            observaciones = "Retira en sala",
            lineas = new[]
            {
                new { productoId = FakeCatalogoClient.ProductoAngulo, cantidad = 2 },
                new { productoId = FakeCatalogoClient.ProductoAngulo, cantidad = 1 }, // se agrupa con la anterior
                new { productoId = FakeCatalogoClient.ProductoBarra, cantidad = 4 }
            }
        });

        Assert.Equal(HttpStatusCode.Created, respuesta.StatusCode);
        Assert.NotNull(respuesta.Headers.Location);
        var nota = await respuesta.Content.ReadFromJsonAsync<NotaVentaDto>();
        Assert.StartsWith("NV-", nota!.Folio);
        Assert.Equal("Pendiente", nota.Estado);
        Assert.Equal(TokenPruebas.VendedorId, nota.VendedorId);
        Assert.Equal(2, nota.Detalles.Count);
        var esperado = 3 * factory.Catalogo.Precio(FakeCatalogoClient.ProductoAngulo)
                       + 4 * factory.Catalogo.Precio(FakeCatalogoClient.ProductoBarra);
        Assert.Equal(esperado, nota.Total);
        Assert.Equal(stockAntes - 3, factory.Catalogo.Stock(FakeCatalogoClient.ProductoAngulo));
    }

    [Fact]
    public async Task Emitir_ConStockInsuficiente_Devuelve422_YNoRegistraLaNota()
    {
        var vendedor = factory.ClienteVendedor();
        var totalAntes = (await vendedor.GetFromJsonAsync<PagedResult<NotaVentaResumenDto>>("/api/v1/notas-venta"))!.Total;

        var respuesta = await vendedor.PostAsJsonAsync("/api/v1/notas-venta",
            NuevaNota(FakeCatalogoClient.ProductoPlanchaPocoStock, 1000));

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
        var totalDespues = (await vendedor.GetFromJsonAsync<PagedResult<NotaVentaResumenDto>>("/api/v1/notas-venta"))!.Total;
        Assert.Equal(totalAntes, totalDespues);
    }

    [Fact]
    public async Task Emitir_ConCatalogoCaido_Devuelve503_YNoRegistraLaNota()
    {
        var vendedor = factory.ClienteVendedor();
        var totalAntes = (await vendedor.GetFromJsonAsync<PagedResult<NotaVentaResumenDto>>("/api/v1/notas-venta"))!.Total;

        factory.Catalogo.Caido = true;
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await vendedor.PostAsJsonAsync("/api/v1/notas-venta", NuevaNota(FakeCatalogoClient.ProductoBarra, 1));
        }
        finally
        {
            factory.Catalogo.Caido = false;
        }

        Assert.Equal(HttpStatusCode.ServiceUnavailable, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
        var totalDespues = (await vendedor.GetFromJsonAsync<PagedResult<NotaVentaResumenDto>>("/api/v1/notas-venta"))!.Total;
        Assert.Equal(totalAntes, totalDespues);
    }

    [Fact]
    public async Task Emitir_ConClienteInexistente_Devuelve404()
    {
        var respuesta = await factory.ClienteVendedor().PostAsJsonAsync("/api/v1/notas-venta",
            NuevaNota(FakeCatalogoClient.ProductoBarra, 1, clienteId: 9999));

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Emitir_SinLineas_Devuelve400()
    {
        var respuesta = await factory.ClienteVendedor().PostAsJsonAsync("/api/v1/notas-venta",
            new { clienteId = 1, lineas = Array.Empty<object>() });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }

    [Fact]
    public async Task Vendedor_NoVeNotasDeOtroVendedor()
    {
        var nota = await EmitirAsync(factory.ClienteVendedor(), FakeCatalogoClient.ProductoBarra, 1);
        var otro = factory.ClienteOtroVendedor();

        var detalle = await otro.GetAsync($"/api/v1/notas-venta/{nota.Id}");
        var lista = await otro.GetFromJsonAsync<PagedResult<NotaVentaResumenDto>>("/api/v1/notas-venta");

        Assert.Equal(HttpStatusCode.NotFound, detalle.StatusCode);
        Assert.DoesNotContain(lista!.Items, n => n.Id == nota.Id);
        Assert.All(lista.Items, n => Assert.Equal(TokenPruebas.OtroVendedorId, n.VendedorId));
    }

    [Fact]
    public async Task Admin_VeNotasDeTodosLosVendedores()
    {
        var nota = await EmitirAsync(factory.ClienteVendedor(), FakeCatalogoClient.ProductoBarra, 1);

        var detalle = await factory.ClienteAdmin().GetAsync($"/api/v1/notas-venta/{nota.Id}");

        Assert.Equal(HttpStatusCode.OK, detalle.StatusCode);
    }

    [Fact]
    public async Task Anular_ReponeElStock()
    {
        var stockAntes = factory.Catalogo.Stock(FakeCatalogoClient.ProductoBarra);
        var nota = await EmitirAsync(factory.ClienteVendedor(), FakeCatalogoClient.ProductoBarra, 5);
        Assert.Equal(stockAntes - 5, factory.Catalogo.Stock(FakeCatalogoClient.ProductoBarra));

        var respuesta = await factory.ClienteAdmin().PatchAsJsonAsync($"/api/v1/notas-venta/{nota.Id}/estado", new { estado = "Anulada" });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var actualizada = await respuesta.Content.ReadFromJsonAsync<NotaVentaDto>();
        Assert.Equal("Anulada", actualizada!.Estado);
        Assert.Equal(stockAntes, factory.Catalogo.Stock(FakeCatalogoClient.ProductoBarra));
    }

    [Fact]
    public async Task CambiarEstado_DeNotaPagada_Devuelve422()
    {
        var admin = factory.ClienteAdmin();
        var nota = await EmitirAsync(factory.ClienteVendedor(), FakeCatalogoClient.ProductoBarra, 1);

        var pagar = await admin.PatchAsJsonAsync($"/api/v1/notas-venta/{nota.Id}/estado", new { estado = "Pagada" });
        var anular = await admin.PatchAsJsonAsync($"/api/v1/notas-venta/{nota.Id}/estado", new { estado = "Anulada" });

        Assert.Equal(HttpStatusCode.OK, pagar.StatusCode);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, anular.StatusCode);
    }

    [Fact]
    public async Task CambiarEstado_ComoVendedor_Devuelve403()
    {
        var vendedor = factory.ClienteVendedor();
        var nota = await EmitirAsync(vendedor, FakeCatalogoClient.ProductoBarra, 1);

        var respuesta = await vendedor.PatchAsJsonAsync($"/api/v1/notas-venta/{nota.Id}/estado", new { estado = "Pagada" });

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task CambiarEstado_AEstadoInvalido_Devuelve400()
    {
        var nota = await EmitirAsync(factory.ClienteVendedor(), FakeCatalogoClient.ProductoBarra, 1);

        var respuesta = await factory.ClienteAdmin().PatchAsJsonAsync($"/api/v1/notas-venta/{nota.Id}/estado", new { estado = "Pendiente" });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
    }
}
