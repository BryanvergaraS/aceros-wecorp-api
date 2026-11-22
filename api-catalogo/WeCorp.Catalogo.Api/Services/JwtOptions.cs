namespace WeCorp.Catalogo.Api.Services;

/// <summary>
/// Configuración del JWT. La llave (Jwt:Key) llega por la variable de entorno Jwt__Key, nunca desde
/// appsettings.json ni el repositorio. La API de Ventas usa la misma llave, emisor y audiencia para validarlo.
/// </summary>
public class JwtOptions
{
    public const string Seccion = "Jwt";
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "WeCorp.Catalogo";
    public string Audience { get; set; } = "WeCorp.Sistema";
    public int ExpiraMinutos { get; set; } = 60;
}
