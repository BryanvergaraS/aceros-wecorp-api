namespace AcerosWeCorp.Api.Models;

public class DetalleNotaVenta
{
    public int Id { get; set; }
    public int NotaVentaId { get; set; }
    public int ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    /// <summary>Columna calculada en SQL Server: Cantidad * PrecioUnitario.</summary>
    public decimal Subtotal { get; private set; }

    public NotaVenta? NotaVenta { get; set; }
    public Producto? Producto { get; set; }
}
