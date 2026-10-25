using Asp.Versioning;
using AcerosWeCorp.Api.Dtos;
using AcerosWeCorp.Api.Exceptions;
using AcerosWeCorp.Api.Repositories;
using AcerosWeCorp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcerosWeCorp.Api.Controllers;

/// <summary>Notas de venta. El Vendedor las emite y ve solo las suyas; el Administrador ve todas y cambia su estado.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notas-venta")]
public class NotasVentaController(INotaVentaRepository notas, INotaVentaService servicio) : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<PagedResult<NotaVentaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? estado, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
    {
        var (p, t) = Paginacion(pagina, tamano);
        return Ok(await notas.ListarAsync(EsAdmin ? null : UsuarioId, estado, p, t));
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<NotaVentaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id)
    {
        var nota = await notas.ObtenerDtoAsync(id);
        // Un vendedor no puede ver notas de otro vendedor: se responde 404 para no revelar que existen.
        if (nota is null || (!EsAdmin && nota.UsuarioId != UsuarioId))
            throw new NotFoundException($"La nota de venta {id} no existe.");
        return Ok(nota);
    }

    /// <summary>Emite una nota de venta: toma el precio del catálogo y descuenta el stock.</summary>
    [HttpPost]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<NotaVentaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear(NotaVentaCrearDto dto)
    {
        var nota = await servicio.CrearAsync(dto, UsuarioId);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nota.Id, version = Version }, nota);
    }

    /// <summary>Marca la nota como Pagada o la anula (al anular, el stock vuelve al catálogo).</summary>
    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType<NotaVentaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> CambiarEstado(int id, CambiarEstadoNotaDto dto) =>
        Ok(await servicio.CambiarEstadoAsync(id, dto.Estado));
}
