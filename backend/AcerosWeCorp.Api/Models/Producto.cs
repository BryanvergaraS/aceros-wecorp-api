namespace AcerosWeCorp.Api.Models;

public class Producto
{
    public const string Tira = "Tira";
    public const string Unidad = "Unidad";

    public int Id { get; set; }
    public int FamiliaId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string UnidadVenta { get; set; } = Tira;
    public decimal? PesoKg { get; set; }
    public decimal PrecioVenta { get; set; }
    public int Stock { get; set; }
    public int StockMinimo { get; set; }
    public bool Estado { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public Familia? Familia { get; set; }
    public ICollection<ProductoProveedor> ProductoProveedores { get; set; } = new List<ProductoProveedor>();
}
