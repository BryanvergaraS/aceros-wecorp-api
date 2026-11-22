namespace WeCorp.Ventas.Api.Models;

/// <summary>Estados de una nota de venta: Pendiente → Pagada o Pendiente → Anulada.</summary>
public static class EstadoNota
{
    public const string Pendiente = "Pendiente";
    public const string Pagada = "Pagada";
    public const string Anulada = "Anulada";

    public static readonly IReadOnlyList<string> Todos = [Pendiente, Pagada, Anulada];
}

/// <summary>
/// Nota de venta. El vendedor vive en la base de la API de Catálogo, por eso se guarda su Id
/// y su nombre (sin clave foránea).
/// </summary>
public class NotaVenta
{
    public int Id { get; set; }
    public string Folio { get; set; } = string.Empty;
    public int ClienteId { get; set; }
    public int VendedorId { get; set; }
    public string VendedorNombre { get; set; } = string.Empty;
    public string Estado { get; set; } = EstadoNota.Pendiente;
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaActualizacion { get; set; }

    public Cliente? Cliente { get; set; }
    public ICollection<DetalleNotaVenta> Detalles { get; set; } = new List<DetalleNotaVenta>();

    /// <summary>Solo una nota Pendiente puede pasar a Pagada o Anulada.</summary>
    public bool PuedeCambiarA(string nuevo) =>
        Estado == EstadoNota.Pendiente && (nuevo == EstadoNota.Pagada || nuevo == EstadoNota.Anulada);

    /// <summary>Suma de Cantidad * PrecioUnitario de todas las líneas.</summary>
    public decimal CalcularTotal() => Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
}

/// <summary>
/// Línea de la nota. Guarda una copia del SKU, nombre y precio del producto al momento de la venta,
/// así el historial no cambia si el catálogo se modifica o el producto se elimina.
/// </summary>
public class DetalleNotaVenta
{
    public int Id { get; set; }
    public int NotaVentaId { get; set; }
    public int ProductoId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    /// <summary>Columna calculada en SQL Server: Cantidad * PrecioUnitario.</summary>
    public decimal Subtotal { get; set; }

    public NotaVenta? NotaVenta { get; set; }
}
