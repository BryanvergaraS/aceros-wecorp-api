namespace AcerosWeCorp.Api.Exceptions;

/// <summary>Error de negocio que el middleware convierte en una respuesta ProblemDetails.</summary>
public abstract class AppException(string mensaje, int statusCode, string titulo) : Exception(mensaje)
{
    public int StatusCode { get; } = statusCode;
    public string Titulo { get; } = titulo;
}

/// <summary>404: el recurso no existe.</summary>
public class NotFoundException(string mensaje) : AppException(mensaje, StatusCodes.Status404NotFound, "Recurso no encontrado");

/// <summary>409: choca con un dato existente (SKU, RUT, email o nombre repetido, o registro en uso).</summary>
public class ConflictException(string mensaje) : AppException(mensaje, StatusCodes.Status409Conflict, "Conflicto");

/// <summary>422: los datos son válidos pero rompen una regla de negocio (por ejemplo, stock insuficiente).</summary>
public class BusinessRuleException(string mensaje) : AppException(mensaje, StatusCodes.Status422UnprocessableEntity, "Regla de negocio");
