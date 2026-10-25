using System.Net;
using System.Net.Http.Json;
using AcerosWeCorp.Api.Dtos;

namespace AcerosWeCorp.Api.Tests;

public class NotasVentaTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    [Fact]
    public async Task Emitir_DescuentaStock_YAnular_LoDevuelve()
    {
        var vendedor = await factory.ClienteVendedorAsync();
        var admin = await factory.ClienteAdminAsync();
        var antes = await vendedor.GetFromJsonAsync<ProductoDto>("/api/v1/productos/7");

        var crear = await vendedor.PostAsJsonAsync("/api/v1/notas-venta", new
        {
            clienteNombre = "Constructora de prueba",
            detalles = new[] { new { productoId = 7, cantidad = 3 } }
        });

        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);
        var nota = await crear.Content.ReadFromJsonAsync<NotaVentaDto>();
        Assert.Equal("Pendiente", nota!.Estado);
        Assert.Equal(3 * antes!.PrecioVenta, nota.Total);

        var despues = await vendedor.GetFromJsonAsync<ProductoDto>("/api/v1/productos/7");
        Assert.Equal(antes.Stock - 3, despues!.Stock);

        var anular = await admin.PatchAsJsonAsync($"/api/v1/notas-venta/{nota.Id}/estado", new { estado = "Anulada" });
        Assert.Equal(HttpStatusCode.OK, anular.StatusCode);
        var final = await vendedor.GetFromJsonAsync<ProductoDto>("/api/v1/productos/7");
        Assert.Equal(antes.Stock, final!.Stock);
    }

    [Fact]
    public async Task Emitir_ConStockInsuficiente_Devuelve422()
    {
        var vendedor = await factory.ClienteVendedorAsync();

        var respuesta = await vendedor.PostAsJsonAsync("/api/v1/notas-venta", new
        {
            clienteNombre = "Cliente de prueba",
            detalles = new[] { new { productoId = 5, cantidad = 100000 } }
        });

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }

    [Fact]
    public async Task CambiarEstado_ComoVendedor_Devuelve403()
    {
        var vendedor = await factory.ClienteVendedorAsync();

        var respuesta = await vendedor.PatchAsJsonAsync("/api/v1/notas-venta/1/estado", new { estado = "Pagada" });

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Middleware_RegistraLasPeticionesEnLogsAuditoria()
    {
        var admin = await factory.ClienteAdminAsync();
        await admin.GetAsync("/api/v1/familias");

        var logs = await admin.GetFromJsonAsync<PagedResult<LogAuditoriaDto>>("/api/v1/logs-auditoria");

        Assert.NotNull(logs);
        Assert.Contains(logs.Items, l => l.RutaEndpoint.StartsWith("/api/v1/familias") && l.CodigoEstadoHttp == 200);
    }
}
