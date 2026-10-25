using Asp.Versioning;
using AcerosWeCorp.Api.Dtos;
using AcerosWeCorp.Api.Exceptions;
using AcerosWeCorp.Api.Models;
using AcerosWeCorp.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcerosWeCorp.Api.Controllers;

/// <summary>Catálogo de productos (recurso principal). Solo el Administrador lo modifica.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/productos")]
public class ProductosController(IProductoRepository productos, IFamiliaRepository familias,
    IProveedorRepository proveedores) : ApiControllerBase
{
    /// <summary>Lista productos con búsqueda por nombre o SKU, filtro por familia y paginación.</summary>
    [HttpGet]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<PagedResult<ProductoDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? buscar, [FromQuery] int? familiaId,
        [FromQuery] bool soloActivos = false, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
    {
        var (p, t) = Paginacion(pagina, tamano);
        return Ok(await productos.BuscarAsync(buscar, familiaId, soloActivos || !EsAdmin, p, t));
    }

    /// <summary>Obtiene un producto por su Id.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<ProductoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id) =>
        Ok(await productos.ObtenerDtoAsync(id) ?? throw new NotFoundException($"El producto {id} no existe."));

    /// <summary>Crea un producto.</summary>
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

    /// <summary>Actualiza un producto.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, ProductoGuardarDto dto)
    {
        var producto = await productos.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"El producto {id} no existe.");
        await ValidarAsync(dto, id);
        Copiar(dto, producto);
        await productos.GuardarCambiosAsync();
        return NoContent();
    }

    /// <summary>Elimina un producto. Si ya tiene ventas, no se puede borrar (se debe desactivar).</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var producto = await productos.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"El producto {id} no existe.");
        if (await productos.TieneVentasAsync(id))
            throw new ConflictException("El producto tiene notas de venta asociadas. Desactívelo en lugar de eliminarlo.");
        productos.Eliminar(producto);
        await productos.GuardarCambiosAsync();
        return NoContent();
    }

    /// <summary>Lista los proveedores del producto con su costo de compra.</summary>
    [HttpGet("{id:int}/proveedores")]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType<List<ProductoProveedorDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ListarProveedores(int id)
    {
        _ = await productos.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"El producto {id} no existe.");
        return Ok(await productos.ListarProveedoresAsync(id));
    }

    /// <summary>Asigna un proveedor al producto (tabla ProductoProveedores).</summary>
    [HttpPost("{id:int}/proveedores")]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AsignarProveedor(int id, ProductoProveedorGuardarDto dto)
    {
        _ = await productos.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"El producto {id} no existe.");
        _ = await proveedores.ObtenerPorIdAsync(dto.ProveedorId) ?? throw new NotFoundException($"El proveedor {dto.ProveedorId} no existe.");
        if (await productos.ObtenerAsignacionAsync(id, dto.ProveedorId) is not null)
            throw new ConflictException("El proveedor ya está asignado a este producto.");

        await productos.AgregarAsignacionAsync(new ProductoProveedor
        {
            ProductoId = id,
            ProveedorId = dto.ProveedorId,
            CostoCompra = dto.CostoCompra
        });
        await productos.GuardarCambiosAsync();
        return NoContent();
    }

    /// <summary>Quita un proveedor del producto.</summary>
    [HttpDelete("{id:int}/proveedores/{proveedorId:int}")]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> QuitarProveedor(int id, int proveedorId)
    {
        var asignacion = await productos.ObtenerAsignacionAsync(id, proveedorId)
            ?? throw new NotFoundException("El proveedor no está asignado a este producto.");
        productos.EliminarAsignacion(asignacion);
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
        p.Estado = dto.Estado;
    }
}
