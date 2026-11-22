using WeCorp.Ventas.Api.Dtos;
using WeCorp.Ventas.Api.Exceptions;
using WeCorp.Ventas.Api.Models;
using WeCorp.Ventas.Api.Repositories;

namespace WeCorp.Ventas.Api.Services;

public interface INotaVentaService
{
    Task<NotaVentaDto> CrearAsync(NotaVentaCrearDto dto, int vendedorId, string vendedorNombre, CancellationToken ct);
    Task<NotaVentaDto?> ObtenerAsync(int id, int? soloVendedorId);
    Task<NotaVentaDto> CambiarEstadoAsync(int id, string nuevoEstado, CancellationToken ct);
}

/// <summary>Reglas de negocio de las notas de venta, coordinadas con la API de Catálogo.</summary>
public class NotaVentaService(
    INotaVentaRepository notas,
    IClienteRepository clientes,
    ICatalogoClient catalogo,
    ILogger<NotaVentaService> logger) : INotaVentaService
{
    public async Task<NotaVentaDto> CrearAsync(NotaVentaCrearDto dto, int vendedorId, string vendedorNombre, CancellationToken ct)
    {
        var cliente = await clientes.ObtenerPorIdAsync(dto.ClienteId)
                      ?? throw new NotFoundException($"El cliente {dto.ClienteId} no existe.");

        // Un mismo producto repetido en varias líneas se suma en una sola.
        var lineas = dto.Lineas
            .GroupBy(l => l.ProductoId)
            .Select(g => new LineaStock(g.Key, g.Sum(l => l.Cantidad)))
            .ToList();

        // 1. Catálogo verifica y descuenta el stock (todo o nada) y entrega el precio vigente.
        var resultado = await catalogo.DescontarStockAsync(lineas, ct);

        var nota = new NotaVenta
        {
            Folio = "TMP-" + Guid.NewGuid().ToString("N")[..16],
            ClienteId = cliente.Id,
            Cliente = cliente,
            VendedorId = vendedorId,
            VendedorNombre = vendedorNombre,
            Estado = EstadoNota.Pendiente,
            Observaciones = string.IsNullOrWhiteSpace(dto.Observaciones) ? null : dto.Observaciones.Trim()
        };
        foreach (var r in resultado)
        {
            nota.Detalles.Add(new DetalleNotaVenta
            {
                ProductoId = r.ProductoId,
                Sku = r.Sku,
                NombreProducto = r.Nombre,
                Cantidad = r.Cantidad,
                PrecioUnitario = r.PrecioUnitario
            });
        }
        nota.Total = nota.CalcularTotal();

        // 2. Se registra la nota. Si falla, se devuelve el stock (compensación).
        try
        {
            await notas.AgregarAsync(nota);
            await notas.GuardarCambiosAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "No se pudo registrar la nota; se repone el stock descontado");
            await CompensarAsync(lineas);
            throw;
        }

        // 3. Folio correlativo a partir del Id generado.
        nota.Folio = $"NV-{nota.Id:D6}";
        await notas.GuardarCambiosAsync();

        return ADto(nota);
    }

    public async Task<NotaVentaDto?> ObtenerAsync(int id, int? soloVendedorId)
    {
        var nota = await notas.ObtenerConDetalleAsync(id);
        if (nota is null) return null;
        // Un vendedor no puede ver notas de otro: se responde como si no existiera.
        if (soloVendedorId is not null && nota.VendedorId != soloVendedorId) return null;
        return ADto(nota);
    }

    public async Task<NotaVentaDto> CambiarEstadoAsync(int id, string nuevoEstado, CancellationToken ct)
    {
        var nota = await notas.ObtenerConDetalleAsync(id, seguimiento: true)
                   ?? throw new NotFoundException($"La nota de venta {id} no existe.");

        if (!nota.PuedeCambiarA(nuevoEstado))
            throw new BusinessRuleException($"La nota {nota.Folio} está {nota.Estado} y ya no puede cambiar de estado.");

        // Al anular, primero se repone el stock; si Catálogo falla, la nota sigue Pendiente (503).
        if (nuevoEstado == EstadoNota.Anulada)
            await catalogo.ReponerStockAsync(nota.Detalles.Select(d => new LineaStock(d.ProductoId, d.Cantidad)), ct);

        nota.Estado = nuevoEstado;
        nota.FechaActualizacion = DateTime.Now;
        await notas.GuardarCambiosAsync();
        return ADto(nota);
    }

    private async Task CompensarAsync(IReadOnlyList<LineaStock> lineas)
    {
        try
        {
            await catalogo.ReponerStockAsync(lineas, CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "No se pudo reponer el stock tras un error al registrar la nota. Revisar manualmente.");
        }
    }

    private static NotaVentaDto ADto(NotaVenta n) => new(
        n.Id, n.Folio, n.ClienteId, n.Cliente?.Rut ?? string.Empty, n.Cliente?.Nombre ?? string.Empty,
        n.VendedorId, n.VendedorNombre, n.Estado, n.Total, n.FechaCreacion,
        n.Observaciones, n.FechaActualizacion,
        n.Detalles.OrderBy(d => d.Id)
            .Select(d => new DetalleNotaDto(d.ProductoId, d.Sku, d.NombreProducto, d.Cantidad, d.PrecioUnitario,
                d.Cantidad * d.PrecioUnitario))
            .ToList());
}
