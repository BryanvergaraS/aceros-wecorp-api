using System.ComponentModel.DataAnnotations;

namespace AcerosWeCorp.Api.Dtos;

public record DetalleNotaVentaDto(int ProductoId, string Sku, string Nombre, int Cantidad, decimal PrecioUnitario, decimal Subtotal);

public record NotaVentaDto(
    int Id, int Folio, int UsuarioId, string Vendedor, string ClienteNombre, string? ClienteRut,
    decimal Total, string Estado, string? Observaciones, DateTime FechaCreacion,
    IReadOnlyList<DetalleNotaVentaDto> Detalles);

public class DetalleNotaVentaCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un producto.")]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 0.")]
    public int Cantidad { get; set; }
}

public class NotaVentaCrearDto
{
    [Required(ErrorMessage = "El nombre del cliente es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre del cliente admite hasta 150 caracteres.")]
    public string ClienteNombre { get; set; } = string.Empty;

    [RegularExpression(@"^\d{1,2}\.?\d{3}\.?\d{3}-[\dkK]$", ErrorMessage = "El RUT debe tener el formato 12.345.678-9.")]
    public string? ClienteRut { get; set; }

    [StringLength(300, ErrorMessage = "Las observaciones admiten hasta 300 caracteres.")]
    public string? Observaciones { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "La nota de venta debe tener al menos un producto.")]
    public List<DetalleNotaVentaCrearDto> Detalles { get; set; } = [];
}

public class CambiarEstadoNotaDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression("^(Pagada|Anulada)$", ErrorMessage = "El estado debe ser Pagada o Anulada.")]
    public string Estado { get; set; } = string.Empty;
}
