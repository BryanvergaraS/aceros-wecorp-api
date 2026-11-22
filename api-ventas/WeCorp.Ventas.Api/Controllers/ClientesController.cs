using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeCorp.Ventas.Api.Dtos;
using WeCorp.Ventas.Api.Exceptions;
using WeCorp.Ventas.Api.Models;
using WeCorp.Ventas.Api.Repositories;
using WeCorp.Ventas.Api.Validacion;

namespace WeCorp.Ventas.Api.Controllers;

/// <summary>Clientes de la barraca (RF06). Ambos roles registran y editan; solo el Administrador elimina.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/clientes")]
public class ClientesController(IClienteRepository clientes) : ApiControllerBase
{
    /// <summary>Lista clientes con búsqueda por RUT o nombre y paginación.</summary>
    [HttpGet]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<PagedResult<ClienteDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] string? buscar, [FromQuery] int pagina = 1, [FromQuery] int tamano = 20)
    {
        var (p, t) = Paginacion(pagina, tamano);
        return Ok(await clientes.BuscarAsync(buscar, p, t));
    }

    /// <summary>Obtiene un cliente por su Id.</summary>
    [HttpGet("{id:int}")]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id) =>
        Ok(await clientes.ObtenerDtoAsync(id) ?? throw new NotFoundException($"El cliente {id} no existe."));

    /// <summary>Registra un cliente con RUT válido y único.</summary>
    [HttpPost]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType<ClienteDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Crear(ClienteGuardarDto dto)
    {
        var rut = RutChileno.Normalizar(dto.Rut)!;
        if (await clientes.ExisteRutAsync(rut))
            throw new ConflictException($"Ya existe un cliente con el RUT {rut}.");

        var cliente = new Cliente();
        Copiar(dto, rut, cliente);
        await clientes.AgregarAsync(cliente);
        await clientes.GuardarCambiosAsync();

        return CreatedAtAction(nameof(ObtenerPorId), new { id = cliente.Id, version = Version },
            await clientes.ObtenerDtoAsync(cliente.Id));
    }

    /// <summary>Actualiza un cliente.</summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = AdminYVendedor)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Actualizar(int id, ClienteGuardarDto dto)
    {
        var cliente = await clientes.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"El cliente {id} no existe.");
        var rut = RutChileno.Normalizar(dto.Rut)!;
        if (await clientes.ExisteRutAsync(rut, id))
            throw new ConflictException($"Ya existe otro cliente con el RUT {rut}.");

        Copiar(dto, rut, cliente);
        await clientes.GuardarCambiosAsync();
        return NoContent();
    }

    /// <summary>Elimina un cliente que no tenga notas de venta.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = SoloAdmin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Eliminar(int id)
    {
        var cliente = await clientes.ObtenerPorIdAsync(id) ?? throw new NotFoundException($"El cliente {id} no existe.");
        if (await clientes.TieneNotasAsync(id))
            throw new ConflictException("El cliente tiene notas de venta asociadas y no se puede eliminar.");

        clientes.Eliminar(cliente);
        await clientes.GuardarCambiosAsync();
        return NoContent();
    }

    private static void Copiar(ClienteGuardarDto dto, string rut, Cliente c)
    {
        c.Rut = rut;
        c.Nombre = dto.Nombre.Trim();
        c.Email = string.IsNullOrWhiteSpace(dto.Email) ? null : dto.Email.Trim().ToLowerInvariant();
        c.Telefono = string.IsNullOrWhiteSpace(dto.Telefono) ? null : dto.Telefono.Trim();
        c.Direccion = string.IsNullOrWhiteSpace(dto.Direccion) ? null : dto.Direccion.Trim();
    }
}
