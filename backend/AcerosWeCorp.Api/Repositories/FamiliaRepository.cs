using AcerosWeCorp.Api.Data;
using AcerosWeCorp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AcerosWeCorp.Api.Repositories;

public interface IFamiliaRepository : IRepository<Familia>
{
    Task<List<Familia>> ListarAsync();
    Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null);
    Task<bool> TieneProductosAsync(int id);
}

public class FamiliaRepository(AppDbContext db) : Repository<Familia>(db), IFamiliaRepository
{
    public Task<List<Familia>> ListarAsync() => Set.AsNoTracking().OrderBy(f => f.Nombre).ToListAsync();

    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null) =>
        Set.AnyAsync(f => f.Nombre == nombre && (excluirId == null || f.Id != excluirId));

    public Task<bool> TieneProductosAsync(int id) => Db.Productos.AnyAsync(p => p.FamiliaId == id);
}
