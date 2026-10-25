using AcerosWeCorp.Api.Data;
using AcerosWeCorp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AcerosWeCorp.Api.Repositories;

public interface IProveedorRepository : IRepository<Proveedor>
{
    Task<List<Proveedor>> ListarAsync(string? texto);
    Task<bool> ExisteRutAsync(string rut, int? excluirId = null);
}

public class ProveedorRepository(AppDbContext db) : Repository<Proveedor>(db), IProveedorRepository
{
    public Task<List<Proveedor>> ListarAsync(string? texto)
    {
        var q = Set.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            q = q.Where(p => p.RazonSocial.Contains(t) || p.Rut.Contains(t));
        }
        return q.OrderBy(p => p.RazonSocial).ToListAsync();
    }

    public Task<bool> ExisteRutAsync(string rut, int? excluirId = null) =>
        Set.AnyAsync(p => p.Rut == rut && (excluirId == null || p.Id != excluirId));
}
