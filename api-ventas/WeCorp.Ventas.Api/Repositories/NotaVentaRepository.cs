using Microsoft.EntityFrameworkCore;
using WeCorp.Ventas.Api.Data;
using WeCorp.Ventas.Api.Dtos;
using WeCorp.Ventas.Api.Models;

namespace WeCorp.Ventas.Api.Repositories;

public interface INotaVentaRepository : IRepository<NotaVenta>
{
    Task<PagedResult<NotaVentaResumenDto>> BuscarAsync(int? vendedorId, string? estado, DateOnly? desde, DateOnly? hasta,
        int pagina, int tamano);

    /// <summary>Nota con su cliente y detalle. Con seguimiento = true se puede modificar y guardar.</summary>
    Task<NotaVenta?> ObtenerConDetalleAsync(int id, bool seguimiento = false);
}

public class NotaVentaRepository(AppDbContext db) : Repository<NotaVenta>(db), INotaVentaRepository
{
    public async Task<PagedResult<NotaVentaResumenDto>> BuscarAsync(int? vendedorId, string? estado,
        DateOnly? desde, DateOnly? hasta, int pagina, int tamano)
    {
        var q = Set.AsNoTracking().AsQueryable();
        if (vendedorId is not null) q = q.Where(n => n.VendedorId == vendedorId);
        if (!string.IsNullOrWhiteSpace(estado)) q = q.Where(n => n.Estado == estado);
        if (desde is not null)
        {
            var inicio = desde.Value.ToDateTime(TimeOnly.MinValue);
            q = q.Where(n => n.FechaCreacion >= inicio);
        }
        if (hasta is not null)
        {
            var fin = hasta.Value.AddDays(1).ToDateTime(TimeOnly.MinValue);
            q = q.Where(n => n.FechaCreacion < fin);
        }

        var total = await q.CountAsync();
        var items = await q.OrderByDescending(n => n.FechaCreacion).ThenByDescending(n => n.Id)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(n => new NotaVentaResumenDto(n.Id, n.Folio, n.ClienteId, n.Cliente!.Rut, n.Cliente.Nombre,
                n.VendedorId, n.VendedorNombre, n.Estado, n.Total, n.FechaCreacion))
            .ToListAsync();
        return new PagedResult<NotaVentaResumenDto>(items, total, pagina, tamano);
    }

    public Task<NotaVenta?> ObtenerConDetalleAsync(int id, bool seguimiento = false)
    {
        var q = Set.Include(n => n.Cliente).Include(n => n.Detalles).AsQueryable();
        if (!seguimiento) q = q.AsNoTracking();
        return q.FirstOrDefaultAsync(n => n.Id == id);
    }
}
