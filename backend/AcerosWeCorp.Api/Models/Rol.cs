namespace AcerosWeCorp.Api.Models;

public class Rol
{
    public const string Administrador = "Administrador";
    public const string Vendedor = "Vendedor";

    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }

    public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
}
