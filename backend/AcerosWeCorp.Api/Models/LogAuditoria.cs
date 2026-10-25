namespace AcerosWeCorp.Api.Models;

public class LogAuditoria
{
    public long Id { get; set; }
    public int? UsuarioId { get; set; }
    public string MetodoHttp { get; set; } = string.Empty;
    public string RutaEndpoint { get; set; } = string.Empty;
    public int CodigoEstadoHttp { get; set; }
    public string? IpOrigen { get; set; }
    public int TiempoRespuestaMs { get; set; }
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    public Usuario? Usuario { get; set; }
}
