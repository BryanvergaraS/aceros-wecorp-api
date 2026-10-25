using AcerosWeCorp.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace AcerosWeCorp.Api.Repositories;

/// <summary>Implementación genérica del patrón Repository sobre EF Core.</summary>
public class Repository<T>(AppDbContext db) : IRepository<T> where T : class
{
    protected readonly AppDbContext Db = db;
    protected DbSet<T> Set => Db.Set<T>();

    public async Task<T?> ObtenerPorIdAsync(params object[] claves) => await Set.FindAsync(claves);

    public async Task AgregarAsync(T entidad) => await Set.AddAsync(entidad);

    public void Actualizar(T entidad) => Set.Update(entidad);

    public void Eliminar(T entidad) => Set.Remove(entidad);

    public Task GuardarCambiosAsync() => Db.SaveChangesAsync();
}
