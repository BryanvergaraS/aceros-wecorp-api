using System.Net;
using System.Net.Http.Json;
using WeCorp.Catalogo.Api.Dtos;

namespace WeCorp.Catalogo.Api.Tests;

/// <summary>CRUD de productos, validación y roles (RF02, RF03).</summary>
public class ProductosTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static object NuevoProducto(string sku, decimal precio = 15990, int stock = 10) => new
    {
        familiaId = 1,
        sku,
        nombre = $"Producto de prueba {sku}",
        unidadVenta = "Tira",
        precioVenta = precio,
        stock,
        stockMinimo = 2,
        activo = true
    };

    [Fact]
    public async Task Listar_ComoVendedor_DevuelveProductosPaginados()
    {
        var cliente = await factory.ClienteVendedorAsync();

        var resultado = await cliente.GetFromJsonAsync<PagedResult<ProductoDto>>("/api/v1/productos?pagina=1&tamano=5");

        Assert.NotNull(resultado);
        Assert.True(resultado.Total >= 8);
        Assert.True(resultado.Items.Count <= 5);
    }

    [Fact]
    public async Task Buscar_PorTexto_FiltraResultados()
    {
        var cliente = await factory.ClienteVendedorAsync();

        var resultado = await cliente.GetFromJsonAsync<PagedResult<ProductoDto>>("/api/v1/productos?buscar=Malla");

        Assert.NotNull(resultado);
        Assert.All(resultado.Items, p => Assert.Contains("Malla", p.Nombre));
    }

    [Fact]
    public async Task Crear_ComoVendedor_Devuelve403()
    {
        var cliente = await factory.ClienteVendedorAsync();

        var respuesta = await cliente.PostAsJsonAsync("/api/v1/productos", NuevoProducto("TEST-403"));

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task CrudCompleto_ComoAdmin()
    {
        var cliente = await factory.ClienteAdminAsync();

        // Crear: 201 + Location
        var crear = await cliente.PostAsJsonAsync("/api/v1/productos", NuevoProducto("TEST-CRUD"));
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);
        Assert.NotNull(crear.Headers.Location);
        var creado = await crear.Content.ReadFromJsonAsync<ProductoDto>();
        Assert.Equal("TEST-CRUD", creado!.Sku);
        Assert.Equal("Abierto", creado.FamiliaNombre);

        // Obtener: 200
        var obtenido = await cliente.GetFromJsonAsync<ProductoDto>($"/api/v1/productos/{creado.Id}");
        Assert.Equal(creado.Id, obtenido!.Id);

        // Actualizar: 204
        var actualizar = await cliente.PutAsJsonAsync($"/api/v1/productos/{creado.Id}", NuevoProducto("TEST-CRUD", 19990, 25));
        Assert.Equal(HttpStatusCode.NoContent, actualizar.StatusCode);
        var actualizado = await cliente.GetFromJsonAsync<ProductoDto>($"/api/v1/productos/{creado.Id}");
        Assert.Equal(19990, actualizado!.PrecioVenta);
        Assert.Equal(25, actualizado.Stock);

        // Eliminar: 204 y luego 404
        var eliminar = await cliente.DeleteAsync($"/api/v1/productos/{creado.Id}");
        Assert.Equal(HttpStatusCode.NoContent, eliminar.StatusCode);
        var despues = await cliente.GetAsync($"/api/v1/productos/{creado.Id}");
        Assert.Equal(HttpStatusCode.NotFound, despues.StatusCode);
    }

    [Fact]
    public async Task Crear_ConSkuRepetido_Devuelve409()
    {
        var cliente = await factory.ClienteAdminAsync();

        var respuesta = await cliente.PostAsJsonAsync("/api/v1/productos", NuevoProducto("ANG-20-20-2"));

        Assert.Equal(HttpStatusCode.Conflict, respuesta.StatusCode);
    }

    [Fact]
    public async Task Crear_ConDatosInvalidos_Devuelve400()
    {
        var cliente = await factory.ClienteAdminAsync();

        var respuesta = await cliente.PostAsJsonAsync("/api/v1/productos", new
        {
            familiaId = 1,
            sku = "",
            nombre = "",
            unidadVenta = "Metro",
            precioVenta = 0,
            stock = -1
        });

        Assert.Equal(HttpStatusCode.BadRequest, respuesta.StatusCode);
        Assert.Equal("application/problem+json", respuesta.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Obtener_Inexistente_Devuelve404()
    {
        var cliente = await factory.ClienteAdminAsync();

        var respuesta = await cliente.GetAsync("/api/v1/productos/999999");

        Assert.Equal(HttpStatusCode.NotFound, respuesta.StatusCode);
    }

    [Fact]
    public async Task Listar_ComoVendedor_NoMuestraInactivos()
    {
        var admin = await factory.ClienteAdminAsync();
        var inactivo = new
        {
            familiaId = 1, sku = "TEST-INACTIVO", nombre = "Producto inactivo de prueba", unidadVenta = "Tira",
            precioVenta = 1000, stock = 5, stockMinimo = 1, activo = false
        };
        (await admin.PostAsJsonAsync("/api/v1/productos", inactivo)).EnsureSuccessStatusCode();
        var vendedor = await factory.ClienteVendedorAsync();

        var paraVendedor = await vendedor.GetFromJsonAsync<PagedResult<ProductoDto>>("/api/v1/productos?buscar=TEST-INACTIVO");
        var paraAdmin = await admin.GetFromJsonAsync<PagedResult<ProductoDto>>("/api/v1/productos?buscar=TEST-INACTIVO");

        Assert.Empty(paraVendedor!.Items);
        Assert.Single(paraAdmin!.Items);
    }

    [Fact]
    public async Task Crear_ConFamiliaInexistente_Devuelve422()
    {
        var cliente = await factory.ClienteAdminAsync();
        var producto = new
        {
            familiaId = 9999, sku = "TEST-FAM", nombre = "Sin familia", unidadVenta = "Tira",
            precioVenta = 1000, stock = 1, stockMinimo = 0, activo = true
        };

        var respuesta = await cliente.PostAsJsonAsync("/api/v1/productos", producto);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, respuesta.StatusCode);
    }
}
