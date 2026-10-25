using Asp.Versioning;
using AcerosWeCorp.Api.Dtos;
using AcerosWeCorp.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcerosWeCorp.Api.Controllers;

/// <summary>Registro de peticiones que guarda el middleware de logging. Solo lectura, solo Administrador.</summary>
[Authorize(Roles = SoloAdmin)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/logs-auditoria")]
public class LogsAuditoriaController(ILogAuditoriaRepository logs) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<PagedResult<LogAuditoriaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] int pagina = 1, [FromQuery] int tamano = 50)
    {
        var (p, t) = Paginacion(pagina, tamano);
        return Ok(await logs.ListarAsync(p, t));
    }
}
