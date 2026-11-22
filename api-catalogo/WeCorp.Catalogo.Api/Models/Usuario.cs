namespace WeCorp.Catalogo.Api.Models;

public class Usuario
{
    public int Id { get; set; }
    public int RolId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public Rol? Rol { get; set; }
}
