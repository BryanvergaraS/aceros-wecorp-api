using System.Net;
using System.Text;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using WeCorp.Ventas.Api.Configuracion;
using WeCorp.Ventas.Api.Dtos;
using WeCorp.Ventas.Api.Exceptions;
using WeCorp.Ventas.Api.Services;

namespace WeCorp.Ventas.Api.Tests;

/// <summary>
/// Pruebas del cliente HTTP tipado hacia la API de Catálogo (RNF06): traducción de errores,
/// reintentos ante fallos y tiempo de espera agotado.
/// </summary>
public class CatalogoClientTests
{
    private static readonly LineaStock[] Lineas = [new LineaStock(1, 2)];

    private const string RespuestaOk =
        "{\"lineas\":[{\"productoId\":1,\"sku\":\"ANG-20-20-2\",\"nombre\":\"Ángulo\",\"precioUnitario\":7990,\"cantidad\":2,\"stockRestante\":38}]}";

    /// <summary>Handler falso: decide la respuesta según el número de llamada.</summary>
    private sealed class HandlerFalso(Func<int, CancellationToken, Task<HttpResponseMessage>> responder) : HttpMessageHandler
    {
        public int Llamadas { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Llamadas++;
            return responder(Llamadas, cancellationToken);
        }
    }

    private static CatalogoClient Crear(HandlerFalso handler, TimeSpan? timeout = null)
    {
        var http = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://catalogo.pruebas/"),
            Timeout = timeout ?? TimeSpan.FromSeconds(5)
        };
        var opciones = Options.Create(new CatalogoOptions { BaseUrl = "http://catalogo.pruebas", TimeoutSegundos = 5, Reintentos = 2 });
        return new CatalogoClient(http, opciones, NullLogger<CatalogoClient>.Instance);
    }

    private static HttpResponseMessage Json(HttpStatusCode status, string json, string tipo = "application/json") =>
        new(status) { Content = new StringContent(json, Encoding.UTF8, tipo) };

    [Fact]
    public async Task Respuesta200_DevuelveLasLineasConPrecio()
    {
        var handler = new HandlerFalso((_, _) => Task.FromResult(Json(HttpStatusCode.OK, RespuestaOk)));

        var resultado = await Crear(handler).DescontarStockAsync(Lineas);

        Assert.Single(resultado);
        Assert.Equal(7990m, resultado[0].PrecioUnitario);
        Assert.Equal(1, handler.Llamadas);
    }

    [Fact]
    public async Task Respuesta422_LanzaBusinessRule_ConElDetalle_YNoReintenta()
    {
        var handler = new HandlerFalso((_, _) => Task.FromResult(Json((HttpStatusCode)422,
            "{\"title\":\"Regla de negocio\",\"status\":422,\"detail\":\"Stock insuficiente para PLA-LIS-2MM\"}",
            "application/problem+json")));

        var ex = await Assert.ThrowsAsync<BusinessRuleException>(() => Crear(handler).DescontarStockAsync(Lineas));

        Assert.Equal("Stock insuficiente para PLA-LIS-2MM", ex.Message);
        Assert.Equal(1, handler.Llamadas);
    }

    [Fact]
    public async Task Respuesta404_LanzaNotFound()
    {
        var handler = new HandlerFalso((_, _) => Task.FromResult(Json(HttpStatusCode.NotFound,
            "{\"title\":\"Recurso no encontrado\",\"status\":404,\"detail\":\"El producto 99 no existe.\"}",
            "application/problem+json")));

        await Assert.ThrowsAsync<NotFoundException>(() => Crear(handler).DescontarStockAsync(Lineas));
    }

    [Fact]
    public async Task Error500_NoReintenta_YLanzaServicioNoDisponible()
    {
        // Catálogo recibió la petición: reintentar podría descontar el stock dos veces.
        var handler = new HandlerFalso((_, _) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.InternalServerError)));

        await Assert.ThrowsAsync<ServicioNoDisponibleException>(() => Crear(handler).DescontarStockAsync(Lineas));

        Assert.Equal(1, handler.Llamadas);
    }

    [Fact]
    public async Task ConexionRechazada_LuegoExito_Reintenta()
    {
        var handler = new HandlerFalso((n, _) => n == 1
            ? Task.FromException<HttpResponseMessage>(new HttpRequestException(HttpRequestError.ConnectionError, "Conexión rechazada"))
            : Task.FromResult(Json(HttpStatusCode.OK, RespuestaOk)));

        var resultado = await Crear(handler).DescontarStockAsync(Lineas);

        Assert.Single(resultado);
        Assert.Equal(2, handler.Llamadas);
    }

    [Fact]
    public async Task ConexionRechazadaPersistente_LanzaServicioNoDisponible_TrasTresIntentos()
    {
        var handler = new HandlerFalso((_, _) =>
            Task.FromException<HttpResponseMessage>(new HttpRequestException(HttpRequestError.ConnectionError, "Conexión rechazada")));

        await Assert.ThrowsAsync<ServicioNoDisponibleException>(() => Crear(handler).DescontarStockAsync(Lineas));

        Assert.Equal(3, handler.Llamadas); // 1 intento + 2 reintentos
    }

    [Fact]
    public async Task TiempoDeEsperaAgotado_LanzaServicioNoDisponible_SinReintentar()
    {
        // Catálogo "no responde": el handler espera hasta que HttpClient cancela por timeout.
        var handler = new HandlerFalso(async (_, ct) =>
        {
            await Task.Delay(Timeout.Infinite, ct);
            return new HttpResponseMessage(HttpStatusCode.OK);
        });

        await Assert.ThrowsAsync<ServicioNoDisponibleException>(
            () => Crear(handler, TimeSpan.FromMilliseconds(100)).ReponerStockAsync(Lineas));

        Assert.Equal(1, handler.Llamadas);
    }

    [Fact]
    public async Task Respuesta409_LanzaConflict()
    {
        var handler = new HandlerFalso((_, _) => Task.FromResult(Json(HttpStatusCode.Conflict,
            "{\"title\":\"Conflicto\",\"status\":409,\"detail\":\"El stock cambió.\"}", "application/problem+json")));

        await Assert.ThrowsAsync<ConflictException>(() => Crear(handler).DescontarStockAsync(Lineas));
    }
}
