using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AcerosWeCorp.Api.Controllers;

/// <summary>Base común: JSON, autenticación obligatoria y utilidades. Cada controlador declara su versión y ruta.</summary>
// Sin [Produces("application/json")]: en .NET 10 ese atributo pisa el Content-Type de los
// ProblemDetails (400 de validación, 401 del login) y saldrían como application/json en vez de
// application/problem+json. Sin él, las respuestas normales igual salen en JSON por defecto.
[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public abstract class ApiControllerBase : ControllerBase
{
    public const string SoloAdmin = Models.Rol.Administrador;
    public const string AdminYVendedor = Models.Rol.Administrador + "," + Models.Rol.Vendedor;

    protected int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    protected bool EsAdmin => User.IsInRole(Models.Rol.Administrador);

    protected string Version => (HttpContext.GetRequestedApiVersion()?.MajorVersion ?? 1).ToString();

    protected static (int Pagina, int Tamano) Paginacion(int pagina, int tamano) =>
        (Math.Max(1, pagina), Math.Clamp(tamano, 1, 100));
}
