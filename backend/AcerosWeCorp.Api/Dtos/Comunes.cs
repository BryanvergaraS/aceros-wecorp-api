namespace AcerosWeCorp.Api.Dtos;

/// <summary>Resultado paginado.</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Pagina, int TamanoPagina);
