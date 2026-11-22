namespace WeCorp.Ventas.Api.Exceptions;

/// <summary>Error de negocio que el middleware convierte en una respuesta ProblemDetails.</summary>
public abstract class AppException(string mensaje, int statusCode, string titulo) : Exception(mensaje)
{
    public int StatusCode { get; } = statusCode;
    public string Titulo { get; } = titulo;
}

/// <summary>404: el recurso no existe.</summary>
public class NotFoundException(string mensaje) : AppException(mensaje, StatusCodes.Status404NotFound, "Recurso no encontrado");

/// <summary>409: RUT repetido o registro en uso.</summary>
public class ConflictException(string mensaje) : AppException(mensaje, StatusCodes.Status409Conflict, "Conflicto");

/// <summary>422: datos válidos que rompen una regla de negocio (stock insuficiente, transición no permitida).</summary>
public class BusinessRuleException(string mensaje) : AppException(mensaje, StatusCodes.Status422UnprocessableEntity, "Regla de negocio");

/// <summary>503: la API de Catálogo no respondió a tiempo o falló.</summary>
public class ServicioNoDisponibleException(string mensaje) : AppException(mensaje, StatusCodes.Status503ServiceUnavailable, "Servicio no disponible");

/// <summary>401: la API de Catálogo rechazó el token reenviado (por ejemplo, venció durante la operación).</summary>
public class NoAutenticadoException(string mensaje) : AppException(mensaje, StatusCodes.Status401Unauthorized, "No autenticado");
