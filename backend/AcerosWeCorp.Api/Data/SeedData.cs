using AcerosWeCorp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AcerosWeCorp.Api.Data;

/// <summary>
/// Datos iniciales del script SQL (roles, familias, productos y proveedores de ejemplo).
/// Se insertan con la migración. Los usuarios no van aquí: ver <see cref="DbInitializer"/>.
/// </summary>
public static class SeedData
{
    private static readonly DateTime FechaSemilla = new(2026, 10, 19);

    public static void Aplicar(ModelBuilder mb)
    {
        mb.Entity<Rol>().HasData(
            new { Id = 1, Nombre = Rol.Administrador, Descripcion = "Acceso total: gestiona catálogo, precios, proveedores y usuarios" },
            new { Id = 2, Nombre = Rol.Vendedor, Descripcion = "Consulta el catálogo y emite notas de venta" });

        mb.Entity<Familia>().HasData(
            new { Id = 1, Nombre = "Abierto", Descripcion = "Perfiles abiertos: ángulos, canales, tees", Estado = true },
            new { Id = 2, Nombre = "Cerrado", Descripcion = "Perfiles cerrados: cuadrados y rectangulares", Estado = true },
            new { Id = 3, Nombre = "Laminado", Descripcion = "Perfiles laminados en caliente", Estado = true },
            new { Id = 4, Nombre = "Viga", Descripcion = "Vigas IPE, HEA y similares", Estado = true },
            new { Id = 5, Nombre = "Plancha", Descripcion = "Planchas lisas y diamantadas", Estado = true },
            new { Id = 6, Nombre = "Malla", Descripcion = "Mallas electrosoldadas", Estado = true },
            new { Id = 7, Nombre = "Barras", Descripcion = "Barras de construcción y lisas", Estado = true });

        mb.Entity<Producto>().HasData(
            Prod(1, 1, "ANG-20-20-2", "Ángulo 20x20x2", Producto.Tira, 7990, 40, 10),
            Prod(2, 1, "ANG-30-30-3", "Ángulo 30x30x3", Producto.Tira, 14990, 35, 10),
            Prod(3, 1, "ANG-50-50-4", "Ángulo 50x50x4", Producto.Tira, 32990, 20, 5),
            Prod(4, 2, "CUA-40-40-2", "Perfil cuadrado 40x40x2", Producto.Tira, 21990, 25, 8),
            Prod(5, 5, "PLA-LIS-2MM", "Plancha lisa 2 mm", Producto.Unidad, 59990, 12, 3),
            Prod(6, 6, "MAL-C92", "Malla C-92", Producto.Unidad, 42990, 15, 5),
            Prod(7, 7, "BAR-12-6M", "Barra estriada 12 mm x 6 m", Producto.Tira, 9990, 80, 20));

        mb.Entity<Proveedor>().HasData(
            new { Id = 1, Rut = "76.111.111-1", RazonSocial = "Aceros del Sur SpA (ejemplo)", Telefono = "+56 9 1111 1111", Email = "ventas@proveedor1.example", Estado = true },
            new { Id = 2, Rut = "76.222.222-2", RazonSocial = "Perfiles Biobío Ltda. (ejemplo)", Telefono = "+56 9 2222 2222", Email = "contacto@proveedor2.example", Estado = true });

        mb.Entity<ProductoProveedor>().HasData(
            new { ProductoId = 1, ProveedorId = 1, CostoCompra = (decimal?)5200m },
            new { ProductoId = 2, ProveedorId = 1, CostoCompra = (decimal?)9800m },
            new { ProductoId = 3, ProveedorId = 2, CostoCompra = (decimal?)21500m },
            new { ProductoId = 4, ProveedorId = 2, CostoCompra = (decimal?)14300m },
            new { ProductoId = 5, ProveedorId = 1, CostoCompra = (decimal?)39000m },
            new { ProductoId = 6, ProveedorId = 2, CostoCompra = (decimal?)28000m },
            new { ProductoId = 7, ProveedorId = 1, CostoCompra = (decimal?)6500m });
    }

    private static object Prod(int id, int familiaId, string sku, string nombre, string unidad,
        decimal precio, int stock, int stockMinimo) => new
    {
        Id = id,
        FamiliaId = familiaId,
        Sku = sku,
        Nombre = nombre,
        UnidadVenta = unidad,
        PrecioVenta = precio,
        Stock = stock,
        StockMinimo = stockMinimo,
        Estado = true,
        FechaCreacion = FechaSemilla
    };
}
