namespace AcerosWeCorp.Api.Models;

public class Familia
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public bool Estado { get; set; } = true;

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
