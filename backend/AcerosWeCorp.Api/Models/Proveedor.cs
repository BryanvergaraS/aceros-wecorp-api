namespace AcerosWeCorp.Api.Models;

public class Proveedor
{
    public int Id { get; set; }
    public string Rut { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public bool Estado { get; set; } = true;

    public ICollection<ProductoProveedor> ProductoProveedores { get; set; } = new List<ProductoProveedor>();
}
