using Asp.Versioning;
using AcerosWeCorp.Api.Dtos;
using AcerosWeCorp.Api.Exceptions;
using AcerosWeCorp.Api.Models;
using AcerosWeCorp.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcerosWeCorp.Api.Controllers;

/// <summary>Proveedores de la barraca. Solo el Administrador los gestiona.</summary>
[Authorize(Roles = SoloAdmin)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/proveedores")]
public class ProveedoresController(IProveedorRepository proveedores) : ApiControllerBase
{
    private static ProveedorDto ADto(Proveedor p) =>
        new(p.Id, p.Rut, p.RazonSocial, p.Telefono, p.Email, p.Direccion, p.Estado);

    [HttpGet]
    [ProducesResponseType<List<ProveedorDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? buscar) =>
        Ok((await proveedores.ListarAsync(buscar)).Select(ADto));

    [HttpGet("{id:int}")]
    [ProducesResponseType<ProveedorDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id) =>
        Ok(ADto(await proveedores.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"El proveedor {id} no existe.")));

    [HttpPost]
    [ProducesResponseType<ProveedorDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(ProveedorGuardarDto dto)
    {
        var rut = dto.Rut.Trim().ToUpperInvariant();
        if (await proveedores.ExisteRutAsync(rut))
            throw new ConflictException($"Ya existe un proveedor con el RUT {rut}.");

        var proveedor = new Proveedor();
        Copiar(dto, proveedor, rut);
        await proveedores.AgregarAsync(proveedor);
        await proveedores.GuardarCambiosAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = proveedor.Id, version = Version }, ADto(proveedor));
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, ProveedorGuardarDto dto)
    {
        var proveedor = await proveedores.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"El proveedor {id} no existe.");
        var rut = dto.Rut.Trim().ToUpperInvariant();
        if (await proveedores.ExisteRutAsync(rut, id))
            throw new ConflictException($"Ya existe un proveedor con el RUT {rut}.");

        Copiar(dto, proveedor, rut);
        await proveedores.GuardarCambiosAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var proveedor = await proveedores.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"El proveedor {id} no existe.");
        proveedores.Eliminar(proveedor);
        await proveedores.GuardarCambiosAsync();
        return NoContent();
    }

    private static void Copiar(ProveedorGuardarDto dto, Proveedor p, string rut)
    {
        p.Rut = rut;
        p.RazonSocial = dto.RazonSocial.Trim();
        p.Telefono = dto.Telefono;
        p.Email = dto.Email;
        p.Direccion = dto.Direccion;
        p.Estado = dto.Estado;
    }
}
