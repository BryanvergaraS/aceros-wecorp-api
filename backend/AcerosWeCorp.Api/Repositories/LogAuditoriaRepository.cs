using AcerosWeCorp.Api.Data;
using AcerosWeCorp.Api.Dtos;
using AcerosWeCorp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AcerosWeCorp.Api.Repositories;

public interface ILogAuditoriaRepository : IRepository<LogAuditoria>
{
    Task<PagedResult<LogAuditoriaDto>> ListarAsync(int pagina, int tamano);
}

public class LogAuditoriaRepository(AppDbContext db) : Repository<LogAuditoria>(db), ILogAuditoriaRepository
{
    public async Task<PagedResult<LogAuditoriaDto>> ListarAsync(int pagina, int tamano)
    {
        var q = Set.AsNoTracking();
        var total = await q.CountAsync();
        var items = await q.OrderByDescending(l => l.Id)
            .Skip((pagina - 1) * tamano).Take(tamano)
            .Select(l => new LogAuditoriaDto(l.Id, l.UsuarioId, l.Usuario != null ? l.Usuario.Email : null,
                l.MetodoHttp, l.RutaEndpoint, l.CodigoEstadoHttp, l.IpOrigen, l.TiempoRespuestaMs, l.FechaRegistro))
            .ToListAsync();
        return new PagedResult<LogAuditoriaDto>(items, total, pagina, tamano);
    }
}
