namespace AcerosWeCorp.Api.Services;

/// <summary>
/// Configuración del JWT. La llave (Jwt:Key) se entrega por variable de entorno
/// (Jwt__Key) o user-secrets, nunca en appsettings.json.
/// </summary>
public class JwtOptions
{
    public const string Seccion = "Jwt";
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "AcerosWeCorp.Api";
    public string Audience { get; set; } = "AcerosWeCorp.Frontend";
    public int ExpiraMinutos { get; set; } = 120;
}
