using AcerosWeCorp.Api.Dtos;
using AcerosWeCorp.Api.Exceptions;
using AcerosWeCorp.Api.Models;
using AcerosWeCorp.Api.Repositories;

namespace AcerosWeCorp.Api.Services;

public interface INotaVentaService
{
    Task<NotaVentaDto> CrearAsync(NotaVentaCrearDto dto, int usuarioId);
    Task<NotaVentaDto> CambiarEstadoAsync(int id, string nuevoEstado);
}

/// <summary>
/// Reglas de las notas de venta: el precio sale del catálogo, el stock se descuenta al emitir
/// y se devuelve al anular. Todo se guarda en un solo SaveChanges (una transacción).
/// </summary>
public class NotaVentaService(INotaVentaRepository notas, IProductoRepository productos) : INotaVentaService
{
    public async Task<NotaVentaDto> CrearAsync(NotaVentaCrearDto dto, int usuarioId)
    {
        // Si un producto viene repetido, se suman sus cantidades.
        var lineas = dto.Detalles
            .GroupBy(d => d.ProductoId)
            .Select(g => new { ProductoId = g.Key, Cantidad = g.Sum(x => x.Cantidad) })
            .ToList();

        var encontrados = (await productos.ObtenerVariosAsync(lineas.Select(l => l.ProductoId)))
            .ToDictionary(p => p.Id);

        var nota = new NotaVenta
        {
            UsuarioId = usuarioId,
            Folio = await notas.SiguienteFolioAsync(),
            ClienteNombre = dto.ClienteNombre.Trim(),
            ClienteRut = string.IsNullOrWhiteSpace(dto.ClienteRut) ? null : dto.ClienteRut.Trim(),
            Observaciones = dto.Observaciones,
            Estado = NotaVenta.Pendiente
        };

        foreach (var linea in lineas)
        {
            if (!encontrados.TryGetValue(linea.ProductoId, out var producto) || !producto.Estado)
                throw new NotFoundException($"El producto {linea.ProductoId} no existe o está inactivo.");
            if (producto.Stock < linea.Cantidad)
                throw new BusinessRuleException(
                    $"Stock insuficiente para {producto.Nombre}: hay {producto.Stock} y se piden {linea.Cantidad}.");

            producto.Stock -= linea.Cantidad;
            nota.Detalles.Add(new DetalleNotaVenta
            {
                ProductoId = producto.Id,
                Cantidad = linea.Cantidad,
                PrecioUnitario = producto.PrecioVenta
            });
            nota.Total += linea.Cantidad * producto.PrecioVenta;
        }

        await notas.AgregarAsync(nota);
        await notas.GuardarCambiosAsync();
        return (await notas.ObtenerDtoAsync(nota.Id))!;
    }

    public async Task<NotaVentaDto> CambiarEstadoAsync(int id, string nuevoEstado)
    {
        var nota = await notas.ObtenerConDetallesAsync(id)
            ?? throw new NotFoundException($"La nota de venta {id} no existe.");

        if (nota.Estado != NotaVenta.Pendiente)
            throw new BusinessRuleException($"La nota de venta ya está {nota.Estado.ToLowerInvariant()} y no se puede cambiar.");

        if (nuevoEstado == NotaVenta.Anulada)
        {
            var devueltos = (await productos.ObtenerVariosAsync(nota.Detalles.Select(d => d.ProductoId)))
                .ToDictionary(p => p.Id);
            foreach (var d in nota.Detalles)
                devueltos[d.ProductoId].Stock += d.Cantidad;
        }

        nota.Estado = nuevoEstado;
        await notas.GuardarCambiosAsync();
        return (await notas.ObtenerDtoAsync(nota.Id))!;
    }
}
