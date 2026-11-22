using System.ComponentModel.DataAnnotations;
using WeCorp.Ventas.Api.Validacion;

namespace WeCorp.Ventas.Api.Dtos;

public record ClienteDto(int Id, string Rut, string Nombre, string? Email, string? Telefono, string? Direccion, DateTime FechaRegistro);

/// <summary>Datos para registrar o editar un cliente.</summary>
public class ClienteGuardarDto
{
    [Required(ErrorMessage = "El RUT es obligatorio.")]
    [StringLength(12, ErrorMessage = "El RUT admite hasta 12 caracteres.")]
    [Rut]
    public string Rut { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre admite hasta 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El email no es válido.")]
    [StringLength(150, ErrorMessage = "El email admite hasta 150 caracteres.")]
    public string? Email { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono admite hasta 20 caracteres.")]
    public string? Telefono { get; set; }

    [StringLength(200, ErrorMessage = "La dirección admite hasta 200 caracteres.")]
    public string? Direccion { get; set; }
}
