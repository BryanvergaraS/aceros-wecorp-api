using System.ComponentModel.DataAnnotations;

namespace AcerosWeCorp.Api.Dtos;

public record ProductoDto(
    int Id, int FamiliaId, string FamiliaNombre, string Sku, string Nombre, string? Descripcion,
    string UnidadVenta, decimal? PesoKg, decimal PrecioVenta, int Stock, int StockMinimo,
    bool Estado, DateTime FechaCreacion);

/// <summary>Datos para crear o editar un producto.</summary>
public class ProductoGuardarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar una familia.")]
    public int FamiliaId { get; set; }

    [Required(ErrorMessage = "El SKU es obligatorio.")]
    [StringLength(50, ErrorMessage = "El SKU admite hasta 50 caracteres.")]
    public string Sku { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre admite hasta 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(300, ErrorMessage = "La descripción admite hasta 300 caracteres.")]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "La unidad de venta es obligatoria.")]
    [RegularExpression("^(Tira|Unidad)$", ErrorMessage = "La unidad de venta debe ser Tira o Unidad.")]
    public string UnidadVenta { get; set; } = "Tira";

    [Range(0.0, 9999999.0, ErrorMessage = "El peso no puede ser negativo.")]
    public decimal? PesoKg { get; set; }

    [Range(1, 999999999999, ErrorMessage = "El precio de venta debe ser mayor que 0.")]
    public decimal PrecioVenta { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "El stock mínimo no puede ser negativo.")]
    public int StockMinimo { get; set; }

    public bool Estado { get; set; } = true;
}

public record ProductoProveedorDto(int ProveedorId, string RazonSocial, decimal? CostoCompra);

public class ProductoProveedorGuardarDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un proveedor.")]
    public int ProveedorId { get; set; }

    [Range(0, 9999999999, ErrorMessage = "El costo de compra no puede ser negativo.")]
    public decimal? CostoCompra { get; set; }
}
