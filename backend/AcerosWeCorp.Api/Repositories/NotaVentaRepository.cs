using AcerosWeCorp.Api.Data;
using AcerosWeCorp.Api.Dtos;
using AcerosWeCorp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AcerosWeCorp.Api.Repositories;

public interface INotaVentaRepository : IRepository<NotaVenta>
{
    Task<PagedResult<NotaVentaDto>> ListarAsync(int? usuarioId, string? estado, int pagina, int tamano);
    Task<NotaVentaDto?> ObtenerDtoAsync(int id);
    Task<NotaVenta?> ObtenerConDetallesAsync(int id);
    Task<int> SiguienteFolioAsync();
}

public class NotaVentaRepository(AppDbContext db) : Repository<NotaVenta>(db), INotaVentaRepository
{
    private static IQueryable<NotaVentaDto> Proyectar(IQueryable<NotaVenta> q) => q.Select(n => new NotaVentaDto(
        n.Id, n.Folio, n.UsuarioId, n.Usuario!.Nombre + " " + n.Usuario!.Apellido, n.ClienteNombre, n.ClienteRut,
        n.Total, n.Estado, n.Observaciones, n.FechaCreacion,
        n.Detalles.Select(d => new DetalleNotaVentaDto(
            d.ProductoId, d.Producto!.Sku, d.Producto!.Nombre, d.Cantidad, d.PrecioUnitario,
            d.Cantidad * d.PrecioUnitario)).ToList()));

    public async Task<PagedResult<NotaVentaDto>> ListarAsync(int? usuarioId, string? estado, int pagina, int tamano)
    {
        var q = Set.AsNoTracking().AsQueryable();
        if (usuarioId is not null) q = q.Where(n => n.UsuarioId == usuarioId);
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(n => n.Estado == estado);

        var total = await q.CountAsync();
        var items = await Proyectar(q.OrderByDescending(n => n.Folio)
            .Skip((pagina - 1) * tamano).Take(tamano)).ToListAsync();
        return new PagedResult<NotaVentaDto>(items, total, pagina, tamano);
    }

    public Task<NotaVentaDto?> ObtenerDtoAsync(int id) =>
        Proyectar(Set.AsNoTracking().Where(n => n.Id == id)).FirstOrDefaultAsync();

    public Task<NotaVenta?> ObtenerConDetallesAsync(int id) =>
        Set.Include(n => n.Detalles).FirstOrDefaultAsync(n => n.Id == id);

    public async Task<int> SiguienteFolioAsync() => (await Set.MaxAsync(n => (int?)n.Folio) ?? 0) + 1;
}
