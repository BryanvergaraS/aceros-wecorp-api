namespace WeCorp.Ventas.Api.Models;

/// <summary>Cliente de la barraca, identificado por su RUT (formato 12345678-5).</summary>
public class Cliente
{
    public int Id { get; set; }
    public string Rut { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public ICollection<NotaVenta> NotasVenta { get; set; } = new List<NotaVenta>();
}
