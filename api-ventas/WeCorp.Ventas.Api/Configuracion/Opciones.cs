namespace WeCorp.Ventas.Api.Configuracion;

/// <summary>
/// Datos para validar el JWT emitido por la API de Catálogo. La llave (Jwt__Key) llega por
/// variable de entorno; emisor y audiencia están en appsettings.json.
/// </summary>
public class JwtOptions
{
    public const string Seccion = "Jwt";
    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = "WeCorp.Catalogo";
    public string Audience { get; set; } = "WeCorp.Sistema";
}

/// <summary>Conexión con la API de Catálogo (Catalogo__BaseUrl por variable de entorno).</summary>
public class CatalogoOptions
{
    public const string Seccion = "Catalogo";
    public string BaseUrl { get; set; } = string.Empty;
    public int TimeoutSegundos { get; set; } = 5;
    public int Reintentos { get; set; } = 2;
}
