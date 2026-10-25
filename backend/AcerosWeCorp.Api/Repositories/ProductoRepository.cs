using AcerosWeCorp.Api.Data;
using AcerosWeCorp.Api.Dtos;
using AcerosWeCorp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AcerosWeCorp.Api.Repositories;

public interface IProductoRepository : IRepository<Producto>
{
    Task<PagedResult<ProductoDto>> BuscarAsync(string? texto, int? familiaId, bool soloActivos, int pagina, int tamano);
    Task<ProductoDto?> ObtenerDtoAsync(int id);
    Task<bool> ExisteSkuAsync(string sku, int? excluirId = null);
    Task<bool> TieneVentasAsync(int id);
    Task<List<Producto>> ObtenerVariosAsync(IEnumerable<int> ids);
    Task<List<ProductoProveedorDto>> ListarProveedoresAsync(int productoId);
    Task<ProductoProveedor?> ObtenerAsignacionAsync(int productoId, int proveedorId);
    Task AgregarAsignacionAsync(ProductoProveedor asignacion);
    void EliminarAsignacion(ProductoProveedor asignacion);
}

public class ProductoRepository(AppDbContext db) : Repository<Producto>(db), IProductoRepository
{
    // Proyección directa a DTO: SQL trae solo las columnas que se usan y sin tracking.
    private static IQueryable<ProductoDto> Proyectar(IQueryable<Producto> q) => q.Select(p => new ProductoDto(
        p.Id, p.FamiliaId, p.Familia!.Nombre, p.Sku, p.Nombre, p.Descripcion, p.UnidadVenta,
        p.PesoKg, p.PrecioVenta, p.Stock, p.StockMinimo, p.Estado, p.FechaCreacion));

    public async Task<PagedResult<ProductoDto>> BuscarAsync(string? texto, int? familiaId, bool soloActivos, int pagina, int tamano)
    {
        var q = Set.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            q = q.Where(p => p.Nombre.Contains(t) || p.Sku.Contains(t));
        }
        if (familiaId is > 0) q = q.Where(p => p.FamiliaId == familiaId);
        if (soloActivos) q = q.Where(p => p.Estado);

        var total = await q.CountAsync();
        var items = await Proyectar(q.OrderBy(p => p.Nombre).ThenBy(p => p.Id)
            .Skip((pagina - 1) * tamano).Take(tamano)).ToListAsync();
        return new PagedResult<ProductoDto>(items, total, pagina, tamano);
    }

    public Task<ProductoDto?> ObtenerDtoAsync(int id) =>
        Proyectar(Set.AsNoTracking().Where(p => p.Id == id)).FirstOrDefaultAsync();

    public Task<bool> ExisteSkuAsync(string sku, int? excluirId = null) =>
        Set.AnyAsync(p => p.Sku == sku && (excluirId == null || p.Id != excluirId));

    public Task<bool> TieneVentasAsync(int id) => Db.DetalleNotasVenta.AnyAsync(d => d.ProductoId == id);

    public Task<List<Producto>> ObtenerVariosAsync(IEnumerable<int> ids)
    {
        var lista = ids.Distinct().ToList();
        return Set.Where(p => lista.Contains(p.Id)).ToListAsync();
    }

    public Task<List<ProductoProveedorDto>> ListarProveedoresAsync(int productoId) =>
        Db.ProductoProveedores.AsNoTracking()
            .Where(pp => pp.ProductoId == productoId)
            .OrderBy(pp => pp.Proveedor!.RazonSocial)
            .Select(pp => new ProductoProveedorDto(pp.ProveedorId, pp.Proveedor!.RazonSocial, pp.CostoCompra))
            .ToListAsync();

    public Task<ProductoProveedor?> ObtenerAsignacionAsync(int productoId, int proveedorId) =>
        Db.ProductoProveedores.FirstOrDefaultAsync(pp => pp.ProductoId == productoId && pp.ProveedorId == proveedorId);

    public async Task AgregarAsignacionAsync(ProductoProveedor asignacion) => await Db.ProductoProveedores.AddAsync(asignacion);

    public void EliminarAsignacion(ProductoProveedor asignacion) => Db.ProductoProveedores.Remove(asignacion);
}
