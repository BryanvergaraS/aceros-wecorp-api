using System.Security.Claims;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WeCorp.Ventas.Api.Models;

namespace WeCorp.Ventas.Api.Controllers;

/// <summary>Base común: autenticación obligatoria (JWT emitido por Catálogo) y utilidades.</summary>
// Sin [Produces("application/json")]: pisaría el Content-Type application/problem+json de los errores.
[ApiController]
[Authorize]
[ProducesResponseType(StatusCodes.Status401Unauthorized)]
public abstract class ApiControllerBase : ControllerBase
{
    public const string SoloAdmin = RolesSistema.Administrador;
    public const string AdminYVendedor = RolesSistema.Administrador + "," + RolesSistema.Vendedor;

    protected int UsuarioId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    protected string UsuarioNombre => User.FindFirstValue(ClaimTypes.Name) ?? "Sin nombre";
    protected bool EsAdmin => User.IsInRole(RolesSistema.Administrador);

    protected string Version => (HttpContext.GetRequestedApiVersion()?.MajorVersion ?? 1).ToString();

    protected static (int Pagina, int Tamano) Paginacion(int pagina, int tamano) =>
        (Math.Max(1, pagina), Math.Clamp(tamano, 1, 50));
}
