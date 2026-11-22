namespace WeCorp.Ventas.Api.Dtos;

// Esquemas del contrato de la API de Catálogo (docs/openapi/catalogo-v1.yaml) que usa el cliente HTTP.

public record LineaStock(int ProductoId, int Cantidad);

public record MovimientoStock(IReadOnlyList<LineaStock> Lineas);

public record LineaStockResultado(int ProductoId, string Sku, string Nombre, decimal PrecioUnitario, int Cantidad, int StockRestante);

public record MovimientoStockResultado(IReadOnlyList<LineaStockResultado> Lineas);
