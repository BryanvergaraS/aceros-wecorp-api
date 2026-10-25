using System.ComponentModel.DataAnnotations;

namespace AcerosWeCorp.Api.Dtos;

public record ProveedorDto(int Id, string Rut, string RazonSocial, string? Telefono, string? Email, string? Direccion, bool Estado);

public class ProveedorGuardarDto
{
    [Required(ErrorMessage = "El RUT es obligatorio.")]
    [RegularExpression(@"^\d{1,2}\.?\d{3}\.?\d{3}-[\dkK]$", ErrorMessage = "El RUT debe tener el formato 76.111.111-1.")]
    public string Rut { get; set; } = string.Empty;

    [Required(ErrorMessage = "La razón social es obligatoria.")]
    [StringLength(150, ErrorMessage = "La razón social admite hasta 150 caracteres.")]
    public string RazonSocial { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono admite hasta 20 caracteres.")]
    public string? Telefono { get; set; }

    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [StringLength(150, ErrorMessage = "El email admite hasta 150 caracteres.")]
    public string? Email { get; set; }

    [StringLength(200, ErrorMessage = "La dirección admite hasta 200 caracteres.")]
    public string? Direccion { get; set; }

    public bool Estado { get; set; } = true;
}
