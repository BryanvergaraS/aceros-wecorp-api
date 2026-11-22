using WeCorp.Ventas.Api.Dtos;
using WeCorp.Ventas.Api.Exceptions;
using WeCorp.Ventas.Api.Services;

namespace WeCorp.Ventas.Api.Tests;

/// <summary>
/// Reemplaza a la API de Catálogo en las pruebas: mantiene productos con stock en memoria
/// y puede simular que el servicio está caído.
/// </summary>
public class FakeCatalogoClient : ICatalogoClient
{
    private sealed class ProductoFalso(string sku, string nombre, decimal precio, int stock)
    {
        public string Sku { get; } = sku;
        public string Nombre { get; } = nombre;
        public decimal Precio { get; } = precio;
        public int Stock { get; set; } = stock;
    }

    public const int ProductoAngulo = 1;
    public const int ProductoBarra = 2;
    public const int ProductoPlanchaPocoStock = 3;

    private readonly object _candado = new();
    private readonly Dictionary<int, ProductoFalso> _productos = new()
    {
        [ProductoAngulo] = new ProductoFalso("ANG-20-20-2", "Ángulo 20x20x2", 7990m, 1000),
        [ProductoBarra] = new ProductoFalso("BAR-12-6M", "Barra estriada 12 mm x 6 m", 9990m, 1000),
        [ProductoPlanchaPocoStock] = new ProductoFalso("PLA-LIS-2MM", "Plancha lisa 2 mm", 59990m, 5)
    };

    /// <summary>Si es true, cada llamada falla como si Catálogo no respondiera.</summary>
    public bool Caido { get; set; }

    public int Stock(int productoId)
    {
        lock (_candado) return _productos[productoId].Stock;
    }

    public decimal Precio(int productoId)
    {
        lock (_candado) return _productos[productoId].Precio;
    }

    public Task<IReadOnlyList<LineaStockResultado>> DescontarStockAsync(IEnumerable<LineaStock> lineas, CancellationToken ct = default)
    {
        if (Caido) throw new ServicioNoDisponibleException(CatalogoClient.MensajeNoDisponible);

        lock (_candado)
        {
            var lista = lineas.ToList();
            // Primero se valida todo; solo después se descuenta (todo o nada).
            foreach (var l in lista)
            {
                if (!_productos.TryGetValue(l.ProductoId, out var p))
                    throw new NotFoundException($"El producto {l.ProductoId} no existe.");
                if (p.Stock < l.Cantidad)
                    throw new BusinessRuleException($"Stock insuficiente para {p.Sku}: disponible {p.Stock}.");
            }

            var resultado = new List<LineaStockResultado>();
            foreach (var l in lista)
            {
                var p = _productos[l.ProductoId];
                p.Stock -= l.Cantidad;
                resultado.Add(new LineaStockResultado(l.ProductoId, p.Sku, p.Nombre, p.Precio, l.Cantidad, p.Stock));
            }
            return Task.FromResult<IReadOnlyList<LineaStockResultado>>(resultado);
        }
    }

    public Task ReponerStockAsync(IEnumerable<LineaStock> lineas, CancellationToken ct = default)
    {
        if (Caido) throw new ServicioNoDisponibleException(CatalogoClient.MensajeNoDisponible);

        lock (_candado)
        {
            foreach (var l in lineas)
            {
                if (!_productos.TryGetValue(l.ProductoId, out var p))
                    throw new NotFoundException($"El producto {l.ProductoId} no existe.");
                p.Stock += l.Cantidad;
            }
        }
        return Task.CompletedTask;
    }
}
