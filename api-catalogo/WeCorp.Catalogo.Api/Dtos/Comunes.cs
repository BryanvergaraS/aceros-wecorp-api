namespace WeCorp.Catalogo.Api.Dtos;

/// <summary>Resultado paginado (items, total, pagina, tamanoPagina).</summary>
public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Pagina, int TamanoPagina);
