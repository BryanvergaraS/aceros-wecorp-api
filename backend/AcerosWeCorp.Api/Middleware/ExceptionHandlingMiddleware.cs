using AcerosWeCorp.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AcerosWeCorp.Api.Middleware;

/// <summary>
/// Middleware propio de manejo de errores: convierte cualquier excepción en una respuesta
/// ProblemDetails (RFC 7807) con el código HTTP que corresponde.
/// </summary>
public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (AppException ex)
        {
            await EscribirAsync(context, ex.StatusCode, ex.Titulo, ex.Message);
        }
        catch (DbUpdateException ex)
        {
            logger.LogWarning(ex, "Error al guardar en la base de datos");
            await EscribirAsync(context, StatusCodes.Status409Conflict, "Conflicto",
                "La operación choca con datos existentes o con una restricción de la base de datos.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error no controlado en {Metodo} {Ruta}", context.Request.Method, context.Request.Path);
            await EscribirAsync(context, StatusCodes.Status500InternalServerError, "Error interno",
                "Ocurrió un error inesperado. Intente nuevamente.");
        }
    }

    private static async Task EscribirAsync(HttpContext context, int status, string titulo, string detalle)
    {
        if (context.Response.HasStarted) return;

        context.Response.Clear();
        context.Response.StatusCode = status;
        var problema = new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = detalle,
            Instance = context.Request.Path
        };
        await context.Response.WriteAsJsonAsync(problema, (System.Text.Json.JsonSerializerOptions?)null, "application/problem+json");
    }
}
