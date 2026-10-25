namespace AcerosWeCorp.Api.Models;

public class NotaVenta
{
    public const string Pendiente = "Pendiente";
    public const string Pagada = "Pagada";
    public const string Anulada = "Anulada";

    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int Folio { get; set; }
    public string ClienteNombre { get; set; } = string.Empty;
    public string? ClienteRut { get; set; }
    public decimal Total { get; set; }
    public string Estado { get; set; } = Pendiente;
    public string? Observaciones { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public Usuario? Usuario { get; set; }
    public ICollection<DetalleNotaVenta> Detalles { get; set; } = new List<DetalleNotaVenta>();
}
