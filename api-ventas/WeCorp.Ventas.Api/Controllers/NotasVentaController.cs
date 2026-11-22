using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeCorp.Ventas.Api.Dtos;
using WeCorp.Ventas.Api.Exceptions;
using WeCorp.Ventas.Api.Models;
using WeCorp.Ventas.Api.Repositories;
using WeCorp.Ventas.Api.Services;

namespace WeCorp.Ventas.Api.Controllers;

/// <summary>Notas de venta (RF07 a RF11). El Vendedor solo ve las propias.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notas-venta")]
public class NotasVentaController(INotaVentaRepository notas, INotaVentaService servicio) : ApiControllerBase
{
    /// <summary>Lista notas con filtros por estado y rango de fechas.</summary>
    [HttpGet]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<PagedResult<NotaVentaResumenDto>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Listar([FromQuery] string? estado, [FromQuery] DateOnly? desde, [FromQuery] DateOnly? hasta,
        [FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
    {
        if (!string.IsNullOrWhiteSpace(estado) && !EstadoNota.Todos.Contains(estado))
        {
            ModelState.AddModelError(nameof(estado), "El estado debe ser Pendiente, Pagada o Anulada.");
            return ValidationProblem(ModelState);
        }

        var (p, t) = Paginacion(pagina, tamano);
        int? vendedor = EsAdmin ? null : UsuarioId;
        return Ok(await notas.BuscarAsync(vendedor, estado, desde, hasta, p, t));
    }

    /// <summary>Obtiene una nota con su detalle.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<NotaVentaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id) =>
        Ok(await servicio.ObtenerAsync(id, EsAdmin ? null : UsuarioId)
           ?? throw new NotFoundException($"La nota de venta {id} no existe."));

    /// <summary>Emite una nota de venta y descuenta el stock en la API de Catálogo.</summary>
    [HttpPost]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<NotaVentaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Crear(NotaVentaCrearDto dto, CancellationToken ct)
    {
        var nota = await servicio.CrearAsync(dto, UsuarioId, UsuarioNombre, ct);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = nota.Id, version = Version }, nota);
    }

    /// <summary>Cambia una nota Pendiente a Pagada o Anulada (al anular se repone el stock).</summary>
    [HttpPatch("{id:int}/estado")]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType<NotaVentaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CambiarEstado(int id, CambioEstadoDto dto, CancellationToken ct) =>
        Ok(await servicio.CambiarEstadoAsync(id, dto.Estado, ct));
}
