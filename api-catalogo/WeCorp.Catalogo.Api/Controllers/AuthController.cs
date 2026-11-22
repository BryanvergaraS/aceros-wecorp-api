using Asp.Versioning;
using WeCorp.Catalogo.Api.Dtos;
using WeCorp.Catalogo.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WeCorp.Catalogo.Api.Controllers;

/// <summary>Inicio de sesión.</summary>
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public class AuthController(IAuthService auth) : ApiControllerBase
{
    /// <summary>Valida email y contraseña y entrega un JWT con el rol del usuario.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType<LoginResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var respuesta = await auth.LoginAsync(request);
        if (respuesta is null)
            return Problem(statusCode: StatusCodes.Status401Unauthorized, title: "Credenciales inválidas",
                detail: "El email o la contraseña no son correctos.");
        return Ok(respuesta);
    }
}
