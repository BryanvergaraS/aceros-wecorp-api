using WeCorp.Catalogo.Api.Dtos;
using WeCorp.Catalogo.Api.Exceptions;
using WeCorp.Catalogo.Api.Repositories;

namespace WeCorp.Catalogo.Api.Services;

public interface IStockService
{
    Task<MovimientoStockResultadoDto> DescontarAsync(MovimientoStockDto movimiento);
    Task<MovimientoStockResultadoDto> ReponerAsync(MovimientoStockDto movimiento);
}

/// <summary>
/// Descuenta o repone stock de varios productos en una sola operación (todo o nada).
/// El stock es token de concurrencia: dos descuentos simultáneos del mismo producto no se pisan (409).
/// Lo consume la API de Ventas al emitir o anular una nota de venta.
/// </summary>
public class StockService(IProductoRepository productos) : IStockService
{
    public Task<MovimientoStockResultadoDto> DescontarAsync(MovimientoStockDto movimiento) =>
        AplicarAsync(movimiento, descontar: true);

    public Task<MovimientoStockResultadoDto> ReponerAsync(MovimientoStockDto movimiento) =>
        AplicarAsync(movimiento, descontar: false);

    private async Task<MovimientoStockResultadoDto> AplicarAsync(MovimientoStockDto movimiento, bool descontar)
    {
        // Si el mismo producto viene en varias líneas, se suman sus cantidades.
        var lineas = movimiento.Lineas
            .GroupBy(l => l.ProductoId)
            .Select(g => (ProductoId: g.Key, Cantidad: g.Sum(l => l.Cantidad)))
            .ToList();

        var encontrados = (await productos.ObtenerVariosAsync(lineas.Select(l => l.ProductoId)))
            .ToDictionary(p => p.Id);

        var faltante = lineas.Select(l => l.ProductoId).FirstOrDefault(id => !encontrados.ContainsKey(id));
        if (descontar && faltante != 0)
            throw new NotFoundException($"El producto {faltante} no existe.");

        // Al reponer se omiten los productos que ya no existen (por ejemplo, eliminados del catálogo
        // después de la venta): así una nota antigua igual se puede anular.
        lineas = lineas.Where(l => encontrados.ContainsKey(l.ProductoId)).ToList();

        // Primero se valida todo; recién después se modifica (todo o nada).
        if (descontar)
        {
            foreach (var (productoId, cantidad) in lineas)
            {
                var p = encontrados[productoId];
                if (!p.Activo)
                    throw new BusinessRuleException($"El producto {p.Sku} está inactivo y no se puede vender.");
                if (!p.TieneStock(cantidad))
                    throw new BusinessRuleException(
                        $"Stock insuficiente para {p.Sku}: disponible {p.Stock}, solicitado {cantidad}.");
            }
        }

        var resultado = new List<LineaStockResultadoDto>();
        foreach (var (productoId, cantidad) in lineas)
        {
            var p = encontrados[productoId];
            p.Stock += descontar ? -cantidad : cantidad;
            resultado.Add(new LineaStockResultadoDto(p.Id, p.Sku, p.Nombre, p.PrecioVenta, cantidad, p.Stock));
        }

        await productos.GuardarCambiosAsync();
        return new MovimientoStockResultadoDto(resultado);
    }
}
