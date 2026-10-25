using Asp.Versioning;
using AcerosWeCorp.Api.Dtos;
using AcerosWeCorp.Api.Exceptions;
using AcerosWeCorp.Api.Models;
using AcerosWeCorp.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcerosWeCorp.Api.Controllers;

/// <summary>Familias del catálogo (Abierto, Cerrado, Viga...).</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/familias")]
public class FamiliasController(IFamiliaRepository familias) : ApiControllerBase
{
    private static FamiliaDto ADto(Familia f) => new(f.Id, f.Nombre, f.Descripcion, f.Estado);

    [HttpGet]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<List<FamiliaDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar() => Ok((await familias.ListarAsync()).Select(ADto));

    [HttpGet("{id:int}")]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<FamiliaDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id) =>
        Ok(ADto(await familias.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"La familia {id} no existe.")));

    [HttpPost]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType<FamiliaDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(FamiliaGuardarDto dto)
    {
        var nombre = dto.Nombre.Trim();
        if (await familias.ExisteNombreAsync(nombre))
            throw new ConflictException($"Ya existe la familia {nombre}.");

        var familia = new Familia { Nombre = nombre, Descripcion = dto.Descripcion, Estado = dto.Estado };
        await familias.AgregarAsync(familia);
        await familias.GuardarCambiosAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = familia.Id, version = Version }, ADto(familia));
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, FamiliaGuardarDto dto)
    {
        var familia = await familias.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"La familia {id} no existe.");
        var nombre = dto.Nombre.Trim();
        if (await familias.ExisteNombreAsync(nombre, id))
            throw new ConflictException($"Ya existe la familia {nombre}.");

        familia.Nombre = nombre;
        familia.Descripcion = dto.Descripcion;
        familia.Estado = dto.Estado;
        await familias.GuardarCambiosAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var familia = await familias.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"La familia {id} no existe.");
        if (await familias.TieneProductosAsync(id))
            throw new ConflictException("La familia tiene productos asociados y no se puede eliminar.");
        familias.Eliminar(familia);
        await familias.GuardarCambiosAsync();
        return NoContent();
    }
}
