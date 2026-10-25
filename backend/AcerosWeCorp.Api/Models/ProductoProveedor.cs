namespace AcerosWeCorp.Api.Models;

/// <summary>Tabla intermedia N a N entre Productos y Proveedores.</summary>
public class ProductoProveedor
{
    public int ProductoId { get; set; }
    public int ProveedorId { get; set; }
    public decimal? CostoCompra { get; set; }

    public Producto? Producto { get; set; }
    public Proveedor? Proveedor { get; set; }
}
