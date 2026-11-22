using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using WeCorp.Ventas.Api.Configuracion;
using WeCorp.Ventas.Api.Dtos;
using WeCorp.Ventas.Api.Exceptions;

namespace WeCorp.Ventas.Api.Services;

/// <summary>Operaciones de la API de Catálogo que necesita la API de Ventas.</summary>
public interface ICatalogoClient
{
    /// <summary>Verifica y descuenta el stock; devuelve el precio vigente de cada producto.</summary>
    Task<IReadOnlyList<LineaStockResultado>> DescontarStockAsync(IEnumerable<LineaStock> lineas, CancellationToken ct = default);

    /// <summary>Repone el stock (anulación de una nota o compensación).</summary>
    Task ReponerStockAsync(IEnumerable<LineaStock> lineas, CancellationToken ct = default);
}

/// <summary>
/// Cliente HTTP tipado (IHttpClientFactory) hacia la API de Catálogo.
/// Cada intento tiene un tiempo de espera (HttpClient.Timeout). Solo se reintenta (hasta Catalogo:Reintentos
/// veces) cuando es seguro que la petición NO llegó a Catálogo (conexión rechazada o nombre no resuelto):
/// descontar y reponer stock no son idempotentes, así que un timeout o un 5xx no se reintentan para no
/// descontar dos veces (ver docs/adr/0003). Los 4xx se traducen al mismo error (404, 409, 422, 401).
/// </summary>
public class CatalogoClient(HttpClient http, IOptions<CatalogoOptions> opciones, ILogger<CatalogoClient> logger) : ICatalogoClient
{
    public const string RutaDescontar = "api/v1/productos/stock/descontar";
    public const string RutaReponer = "api/v1/productos/stock/reponer";
    public const string MensajeNoDisponible = "El servicio de catálogo no está disponible. Intente nuevamente.";

    public async Task<IReadOnlyList<LineaStockResultado>> DescontarStockAsync(IEnumerable<LineaStock> lineas, CancellationToken ct = default)
    {
        var resultado = await EnviarAsync(RutaDescontar, lineas, ct);
        return resultado.Lineas;
    }

    public async Task ReponerStockAsync(IEnumerable<LineaStock> lineas, CancellationToken ct = default) =>
        await EnviarAsync(RutaReponer, lineas, ct);

    private async Task<MovimientoStockResultado> EnviarAsync(string ruta, IEnumerable<LineaStock> lineas, CancellationToken ct)
    {
        var cuerpo = new MovimientoStock(lineas.ToList());
        var reintentos = Math.Max(0, opciones.Value.Reintentos);

        for (var intento = 0; ; intento++)
        {
            try
            {
                using var respuesta = await http.PostAsJsonAsync(ruta, cuerpo, ct);

                if (respuesta.IsSuccessStatusCode)
                    return await respuesta.Content.ReadFromJsonAsync<MovimientoStockResultado>(ct)
                           ?? throw new ServicioNoDisponibleException(MensajeNoDisponible);

                if ((int)respuesta.StatusCode < 500)
                    throw await TraducirErrorAsync(respuesta, ct);

                // 5xx: Catálogo recibió la petición; no se reintenta porque no sabemos si alcanzó a aplicarla.
                logger.LogError("La API de Catálogo respondió {Status} en {Ruta}", (int)respuesta.StatusCode, ruta);
                throw new ServicioNoDisponibleException(MensajeNoDisponible);
            }
            catch (HttpRequestException ex) when (NoLlegoACatalogo(ex) && intento < reintentos)
            {
                // La conexión ni siquiera se estableció: es seguro reintentar.
                logger.LogWarning(ex, "No se pudo conectar con la API de Catálogo (intento {Intento})", intento + 1);
                await Task.Delay(TimeSpan.FromMilliseconds(200 * (intento + 1)), ct);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "Falló la comunicación con la API de Catálogo en {Ruta}", ruta);
                throw new ServicioNoDisponibleException(MensajeNoDisponible);
            }
            catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
            {
                // HttpClient.Timeout agotado (no fue el llamador quien canceló). No se reintenta.
                logger.LogError(ex, "La API de Catálogo no respondió a tiempo en {Ruta}", ruta);
                throw new ServicioNoDisponibleException(MensajeNoDisponible);
            }
            catch (System.Text.Json.JsonException ex)
            {
                logger.LogError(ex, "La API de Catálogo devolvió una respuesta inválida en {Ruta}", ruta);
                throw new ServicioNoDisponibleException(MensajeNoDisponible);
            }
        }
    }

    /// <summary>True si el error ocurrió antes de que la petición llegara a Catálogo.</summary>
    private static bool NoLlegoACatalogo(HttpRequestException ex) =>
        ex.HttpRequestError is HttpRequestError.ConnectionError or HttpRequestError.NameResolutionError;

    private async Task<Exception> TraducirErrorAsync(HttpResponseMessage respuesta, CancellationToken ct)
    {
        string? detalle = null;
        try
        {
            var problema = await respuesta.Content.ReadFromJsonAsync<ProblemDetails>(ct);
            detalle = problema?.Detail ?? problema?.Title;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // La respuesta no traía ProblemDetails: se usa un mensaje genérico.
        }

        switch (respuesta.StatusCode)
        {
            case HttpStatusCode.NotFound:
                return new NotFoundException(detalle ?? "Un producto de la nota no existe en el catálogo.");
            case HttpStatusCode.UnprocessableEntity:
            case HttpStatusCode.BadRequest:
                return new BusinessRuleException(detalle ?? "El catálogo rechazó la operación de stock.");
            case HttpStatusCode.Conflict:
                return new ConflictException(detalle ?? "El stock cambió mientras se procesaba la venta. Intente nuevamente.");
            case HttpStatusCode.Unauthorized:
                return new NoAutenticadoException("La sesión expiró. Inicie sesión nuevamente.");
            default:
                logger.LogError("La API de Catálogo respondió {Status}: {Detalle}", (int)respuesta.StatusCode, detalle);
                return new ServicioNoDisponibleException(MensajeNoDisponible);
        }
    }
}
