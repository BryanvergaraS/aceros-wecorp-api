using System.ComponentModel.DataAnnotations;

namespace WeCorp.Catalogo.Api.Dtos;

public class LineaStockDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Debe indicar un producto.")]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que 0.")]
    public int Cantidad { get; set; }
}

/// <summary>Movimiento de stock de varios productos (lo usa la API de Ventas).</summary>
public class MovimientoStockDto
{
    [Required(ErrorMessage = "Debe indicar al menos un producto.")]
    [MinLength(1, ErrorMessage = "Debe indicar al menos un producto.")]
    public List<LineaStockDto> Lineas { get; set; } = new();
}

public record LineaStockResultadoDto(int ProductoId, string Sku, string Nombre, decimal PrecioUnitario,
    int Cantidad, int StockRestante);

public record MovimientoStockResultadoDto(IReadOnlyList<LineaStockResultadoDto> Lineas);
