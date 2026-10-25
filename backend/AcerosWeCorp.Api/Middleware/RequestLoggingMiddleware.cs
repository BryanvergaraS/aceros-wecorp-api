using System.Diagnostics;
using System.Security.Claims;
using AcerosWeCorp.Api.Models;
using AcerosWeCorp.Api.Repositories;

namespace AcerosWeCorp.Api.Middleware;

/// <summary>
/// Middleware propio de logging: mide cada petición a /api y la guarda en la tabla
/// LogsAuditoria (método, ruta, código HTTP, IP, tiempo de respuesta y usuario).
/// </summary>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger,
    IServiceScopeFactory scopeFactory)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await next(context);
            return;
        }

        var reloj = Stopwatch.StartNew();
        var huboError = false;
        try
        {
            await next(context);
        }
        catch
        {
            huboError = true;
            throw;
        }
        finally
        {
            reloj.Stop();
            var status = huboError ? StatusCodes.Status500InternalServerError : context.Response.StatusCode;
            var ruta = $"{context.Request.Path}{context.Request.QueryString}";
            logger.LogInformation("{Metodo} {Ruta} -> {Status} en {Ms} ms",
                context.Request.Method, ruta, status, reloj.ElapsedMilliseconds);

            await GuardarAsync(new LogAuditoria
            {
                UsuarioId = int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null,
                MetodoHttp = context.Request.Method,
                RutaEndpoint = ruta.Length > 300 ? ruta[..300] : ruta,
                CodigoEstadoHttp = status,
                IpOrigen = context.Connection.RemoteIpAddress?.ToString(),
                TiempoRespuestaMs = (int)reloj.ElapsedMilliseconds,
                FechaRegistro = DateTime.Now
            });
        }
    }

    // Se usa un scope propio para no mezclar el log con los cambios (o errores) de la petición.
    private async Task GuardarAsync(LogAuditoria log)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<ILogAuditoriaRepository>();
            await repo.AgregarAsync(log);
            await repo.GuardarCambiosAsync();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "No se pudo guardar el log de auditoría");
        }
    }
}
