using System.Net.Http.Headers;

namespace WeCorp.Ventas.Api.Services;

/// <summary>
/// Copia el JWT de la petición actual en las llamadas a la API de Catálogo: así Catálogo
/// autoriza con el mismo usuario y rol que llamó a Ventas.
/// </summary>
public class ReenviarTokenHandler(IHttpContextAccessor accesor) : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var encabezado = accesor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(encabezado) && AuthenticationHeaderValue.TryParse(encabezado, out var valor))
            request.Headers.Authorization = valor;

        return base.SendAsync(request, cancellationToken);
    }
}
