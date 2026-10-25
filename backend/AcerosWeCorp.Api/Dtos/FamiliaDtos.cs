using System.ComponentModel.DataAnnotations;

namespace AcerosWeCorp.Api.Dtos;

public record FamiliaDto(int Id, string Nombre, string? Descripcion, bool Estado);

public class FamiliaGuardarDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre admite hasta 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "La descripción admite hasta 200 caracteres.")]
    public string? Descripcion { get; set; }

    public bool Estado { get; set; } = true;
}
