using System.ComponentModel.DataAnnotations;

namespace AcerosWeCorp.Api.Dtos;

public class LoginRequest
{
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Password { get; set; } = string.Empty;
}

public record LoginResponse(string Token, DateTime Expira, UsuarioDto Usuario);
