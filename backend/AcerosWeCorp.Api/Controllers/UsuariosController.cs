using Asp.Versioning;
using AcerosWeCorp.Api.Dtos;
using AcerosWeCorp.Api.Exceptions;
using AcerosWeCorp.Api.Models;
using AcerosWeCorp.Api.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcerosWeCorp.Api.Controllers;

/// <summary>Usuarios del sistema. Solo el Administrador los gestiona.</summary>
[Authorize(Roles = SoloAdmin)]
[ProducesResponseType(StatusCodes.Status403Forbidden)]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/usuarios")]
public class UsuariosController(IUsuarioRepository usuarios) : ApiControllerBase
{
    [HttpGet]
    [ProducesResponseType<List<UsuarioDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar() => Ok(await usuarios.ListarAsync());

    [HttpGet("{id:int}")]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerPorId(int id) =>
        Ok(await usuarios.ObtenerDtoAsync(id) ?? throw new NotFoundException($"El usuario {id} no existe."));

    [HttpGet("roles")]
    [ProducesResponseType<List<RolDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarRoles() => Ok(await usuarios.ListarRolesAsync());

    [HttpPost]
    [ProducesResponseType<UsuarioDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Crear(UsuarioCrearDto dto)
    {
        var email = dto.Email.Trim().ToLowerInvariant();
        if (!await usuarios.ExisteRolAsync(dto.RolId))
            throw new BusinessRuleException($"El rol {dto.RolId} no existe.");
        if (await usuarios.ExisteEmailAsync(email))
            throw new ConflictException($"Ya existe un usuario con el email {email}.");

        var usuario = new Usuario
        {
            RolId = dto.RolId,
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            Email = email,
            Telefono = dto.Telefono,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };
        await usuarios.AgregarAsync(usuario);
        await usuarios.GuardarCambiosAsync();
        return CreatedAtAction(nameof(ObtenerPorId), new { id = usuario.Id, version = Version },
            await usuarios.ObtenerDtoAsync(usuario.Id));
    }
}
