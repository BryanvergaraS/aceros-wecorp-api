using AcerosWeCorp.Api.Data;
using AcerosWeCorp.Api.Dtos;
using AcerosWeCorp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AcerosWeCorp.Api.Repositories;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObtenerPorEmailAsync(string email);
    Task<List<UsuarioDto>> ListarAsync();
    Task<UsuarioDto?> ObtenerDtoAsync(int id);
    Task<bool> ExisteEmailAsync(string email);
    Task<List<RolDto>> ListarRolesAsync();
    Task<bool> ExisteRolAsync(int rolId);
}

public class UsuarioRepository(AppDbContext db) : Repository<Usuario>(db), IUsuarioRepository
{
    private static IQueryable<UsuarioDto> Proyectar(IQueryable<Usuario> q) => q.Select(u => new UsuarioDto(
        u.Id, u.Nombre, u.Apellido, u.Email, u.Telefono, u.Rol!.Nombre, u.Estado, u.FechaRegistro));

    public Task<Usuario?> ObtenerPorEmailAsync(string email) =>
        Set.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Email == email);

    public Task<List<UsuarioDto>> ListarAsync() =>
        Proyectar(Set.AsNoTracking().OrderBy(u => u.Nombre)).ToListAsync();

    public Task<UsuarioDto?> ObtenerDtoAsync(int id) =>
        Proyectar(Set.AsNoTracking().Where(u => u.Id == id)).FirstOrDefaultAsync();

    public Task<bool> ExisteEmailAsync(string email) => Set.AnyAsync(u => u.Email == email);

    public Task<List<RolDto>> ListarRolesAsync() =>
        Db.Roles.AsNoTracking().OrderBy(r => r.Id).Select(r => new RolDto(r.Id, r.Nombre, r.Descripcion)).ToListAsync();

    public Task<bool> ExisteRolAsync(int rolId) => Db.Roles.AnyAsync(r => r.Id == rolId);
}
