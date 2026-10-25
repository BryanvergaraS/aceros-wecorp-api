using System.ComponentModel.DataAnnotations;

namespace AcerosWeCorp.Api.Dtos;

public record UsuarioDto(int Id, string Nombre, string Apellido, string Email, string? Telefono, string Rol, bool Estado, DateTime FechaRegistro);

public class UsuarioCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un rol.")]
    public int RolId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(60)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [StringLength(150)]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [StringLength(20)]
    public string? Telefono { get; set; }
}

public record RolDto(int Id, string Nombre, string? Descripcion);

public record LogAuditoriaDto(long Id, int? UsuarioId, string? UsuarioEmail, string MetodoHttp, string RutaEndpoint,
    int CodigoEstadoHttp, string? IpOrigen, int TiempoRespuestaMs, DateTime FechaRegistro);
