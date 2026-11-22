using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeCorp.Catalogo.Api.Dtos;
using WeCorp.Catalogo.Api.Services;

namespace WeCorp.Catalogo.Api.Controllers;

/// <summary>
/// Movimientos de stock. Los consume la API de Ventas (integración entre servicios),
/// reenviando el JWT del usuario que emite o anula la nota.
/// </summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/productos/stock")]
[Authorize(Roles = AdminYVendedor)]
public class StockController(IStockService stock) : ApiControllerBase
{
    /// <summary>Verifica y descuenta stock de varios productos; todo o nada (RF08).</summary>
    [HttpPost("descontar")]
    [ProducesResponseType<MovimientoStockResultadoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Descontar(MovimientoStockDto movimiento) =>
        Ok(await stock.DescontarAsync(movimiento));

    /// <summary>Repone stock de varios productos: anulación de una nota o compensación (RF11).</summary>
    [HttpPost("reponer")]
    [ProducesResponseType<MovimientoStockResultadoDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reponer(MovimientoStockDto movimiento) =>
        Ok(await stock.ReponerAsync(movimiento));
}
