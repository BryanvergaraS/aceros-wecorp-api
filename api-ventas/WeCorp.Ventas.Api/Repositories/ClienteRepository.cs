using Microsoft.EntityFrameworkCore;
using WeCorp.Ventas.Api.Data;
using WeCorp.Ventas.Api.Dtos;
using WeCorp.Ventas.Api.Models;

namespace WeCorp.Ventas.Api.Repositories;

public interface IClienteRepository : IRepository<Cliente>
{
    Task<PagedResult<ClienteDto>> BuscarAsync(string? texto, int pagina, int tamano);
    Task<ClienteDto?> ObtenerDtoAsync(int id);
    Task<bool> ExisteRutAsync(string rut, int? excluirId = null);
    Task<bool> TieneNotasAsync(int id);
}

public class ClienteRepository(AppDbContext db) : Repository<Cliente>(db), IClienteRepository
{
    private static IQueryable<ClienteDto> Proyectar(IQueryable<Cliente> q) => q.Select(c => new ClienteDto(
        c.Id, c.Rut, c.Nombre, c.Email, c.Telefono, c.Direccion, c.FechaRegistro));

    public async Task<PagedResult<ClienteDto>> BuscarAsync(string? texto, int pagina, int tamano)
    {
        var q = Set.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            var rut = t.Replace(".", "");
            q = q.Where(c => c.Nombre.Contains(t) || c.Rut.Contains(rut));
        }

        var total = await q.CountAsync();
        var items = await Proyectar(q.OrderBy(c => c.Nombre).ThenBy(c => c.Id)
            .Skip((pagina - 1) * tamano).Take(tamano)).ToListAsync();
        return new PagedResult<ClienteDto>(items, total, pagina, tamano);
    }

    public Task<ClienteDto?> ObtenerDtoAsync(int id) =>
        Proyectar(Set.AsNoTracking().Where(c => c.Id == id)).FirstOrDefaultAsync();

    public Task<bool> ExisteRutAsync(string rut, int? excluirId = null) =>
        Set.AnyAsync(c => c.Rut == rut && (excluirId == null || c.Id != excluirId));

    public Task<bool> TieneNotasAsync(int id) => Db.NotasVenta.AnyAsync(n => n.ClienteId == id);
}
