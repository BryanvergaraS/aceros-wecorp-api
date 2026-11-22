using System.ComponentModel.DataAnnotations;

namespace WeCorp.Catalogo.Api.Dtos;

public record UsuarioDto(int Id, string Nombre, string Apellido, string Email, string? Telefono,
    string Rol, bool Activo, DateTime FechaRegistro);

public record RolDto(int Id, string Nombre, string? Descripcion);

public class UsuarioCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un rol.")]
    public int RolId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "El nombre admite hasta 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(60, ErrorMessage = "El apellido admite hasta 60 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [StringLength(150, ErrorMessage = "El email admite hasta 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string Password { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono admite hasta 20 caracteres.")]
    public string? Telefono { get; set; }
}

public class UsuarioActualizarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un rol.")]
    public int RolId { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "El nombre admite hasta 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(60, ErrorMessage = "El apellido admite hasta 60 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono admite hasta 20 caracteres.")]
    public string? Telefono { get; set; }

    public bool Activo { get; set; } = true;
}
