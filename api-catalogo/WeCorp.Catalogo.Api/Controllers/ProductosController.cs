using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeCorp.Catalogo.Api.Dtos;
using WeCorp.Catalogo.Api.Exceptions;
using WeCorp.Catalogo.Api.Models;
using WeCorp.Catalogo.Api.Repositories;

namespace WeCorp.Catalogo.Api.Controllers;

/// <summary>Catálogo de productos (recurso con CRUD completo). Solo el Administrador lo modifica.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/productos")]
public class ProductosController(IProductoRepository productos, IFamiliaRepository familias) : ApiControllerBase
{
    /// <summary>Lista productos con búsqueda por nombre o SKU, filtro por familia y paginación (RF02).</summary>
    /// <remarks>El Vendedor solo ve productos activos; el Administrador ve todos.</remarks>
    [HttpGet]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<PagedResult<ProductoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? buscar, [FromQuery] int? familiaId,
        [FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
    {
        var (p, t) = Paginacion(pagina, tamano);
        return Ok(await productos.BuscarAsync(buscar, familiaId, soloActivos: !EsAdmin, p, t));
    }

    /// <summary>Obtiene un producto por su Id.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<ProductoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id) =>
        Ok(await productos.ObtenerDtoAsync(id) ?? throw new NotFoundException($"El producto {id} no existe."));

    /// <summary>Crea un producto (RF03).</summary>
    [HttpPost]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType<ProductoDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear(ProductoGuardarDto dto)
    {
        await ValidarAsync(dto, null);
        var producto = new Producto();
        Copiar(dto, producto);
        await productos.AgregarAsync(producto);
        await productos.GuardarCambiosAsync();

        var creado = await productos.ObtenerDtoAsync(producto.Id);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = producto.Id, version = Version }, creado);
    }

    /// <summary>Actualiza un producto (RF03).</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Actualizar(int id, ProductoGuardarDto dto)
    {
        var producto = await productos.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"El producto {id} no existe.");
        await ValidarAsync(dto, id);
        Copiar(dto, producto);
        await productos.GuardarCambiosAsync();
        return NoContent();
    }

    /// <summary>Elimina un producto (RF03). Las notas de venta conservan una copia de sus datos.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var producto = await productos.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"El producto {id} no existe.");
        productos.Eliminar(producto);
        await productos.GuardarCambiosAsync();
        return NoContent();
    }

    private async Task ValidarAsync(ProductoGuardarDto dto, int? id)
    {
        if (await familias.ObtenerPorIdAsync(dto.FamiliaId) is null)
            throw new BusinessRuleException($"La familia {dto.FamiliaId} no existe.");
        if (await productos.ExisteSkuAsync(dto.Sku.Trim(), id))
            throw new ConflictException($"Ya existe un producto con el SKU {dto.Sku.Trim()}.");
    }

    private static void Copiar(ProductoGuardarDto dto, Producto p)
    {
        p.FamiliaId = dto.FamiliaId;
        p.Sku = dto.Sku.Trim();
        p.Nombre = dto.Nombre.Trim();
        p.Descripcion = string.IsNullOrWhiteSpace(dto.Descripcion) ? null : dto.Descripcion.Trim();
        p.UnidadVenta = dto.UnidadVenta;
        p.PesoKg = dto.PesoKg;
        p.PrecioVenta = Math.Round(dto.PrecioVenta, 0);
        p.Stock = dto.Stock;
        p.StockMinimo = dto.StockMinimo;
        p.Activo = dto.Activo;
    }
}
