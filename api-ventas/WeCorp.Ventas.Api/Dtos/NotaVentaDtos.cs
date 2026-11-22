using System.ComponentModel.DataAnnotations;

namespace WeCorp.Ventas.Api.Dtos;

public record DetalleNotaDto(int ProductoId, string Sku, string NombreProducto, int Cantidad, decimal PrecioUnitario, decimal Subtotal);

public record NotaVentaResumenDto(
    int Id, string Folio, int ClienteId, string ClienteRut, string ClienteNombre,
    int VendedorId, string VendedorNombre, string Estado, decimal Total, DateTime FechaCreacion);

public record NotaVentaDto(
    int Id, string Folio, int ClienteId, string ClienteRut, string ClienteNombre,
    int VendedorId, string VendedorNombre, string Estado, decimal Total, DateTime FechaCreacion,
    string? Observaciones, DateTime? FechaActualizacion, IReadOnlyList<DetalleNotaDto> Detalles);

/// <summary>Una línea de la nota: producto del catálogo y cantidad.</summary>
public class LineaNotaDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un producto.")]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 0.")]
    public int Cantidad { get; set; }
}

/// <summary>Datos para emitir una nota de venta. El precio lo entrega la API de Catálogo.</summary>
public class NotaVentaCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un cliente.")]
    public int ClienteId { get; set; }

    [StringLength(300, ErrorMessage = "Las observaciones admiten hasta 300 caracteres.")]
    public string? Observaciones { get; set; }

    [Required(ErrorMessage = "Debe agregar al menos un producto.")]
    [MinLength(1, ErrorMessage = "Debe agregar al menos un producto.")]
    public List<LineaNotaDto> Lineas { get; set; } = new();
}

/// <summary>Nuevo estado de la nota: Pagada o Anulada.</summary>
public class CambioEstadoDto
{
    [Required(ErrorMessage = "El estado es obligatorio.")]
    [RegularExpression("^(Pagada|Anulada)$", ErrorMessage = "El estado debe ser Pagada o Anulada.")]
    public string Estado { get; set; } = string.Empty;
}
