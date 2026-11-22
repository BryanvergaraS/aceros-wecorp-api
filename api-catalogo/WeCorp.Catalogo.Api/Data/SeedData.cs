using Microsoft.EntityFrameworkCore;
using WeCorp.Catalogo.Api.Models;

namespace WeCorp.Catalogo.Api.Data;

/// <summary>
/// Datos iniciales (roles, familias y productos de ejemplo de We-Corp). Se insertan con la migración.
/// Los usuarios no van aquí porque sus contraseñas son secretos: ver <see cref="DbInitializer"/>.
/// </summary>
public static class SeedData
{
    private static readonly DateTime FechaSemilla = new(2026, 11, 9);

    public static void Aplicar(ModelBuilder mb)
    {
        mb.Entity<Rol>().HasData(
            new { Id = 1, Nombre = Rol.Administrador, Descripcion = "Gestiona catálogo, usuarios y estado de las notas de venta" },
            new { Id = 2, Nombre = Rol.Vendedor, Descripcion = "Consulta el catálogo, registra clientes y emite notas de venta" });

        mb.Entity<Familia>().HasData(
            new { Id = 1, Nombre = "Abierto", Descripcion = "Perfiles abiertos: ángulos, canales, tees", Activo = true },
            new { Id = 2, Nombre = "Cerrado", Descripcion = "Perfiles cerrados: cuadrados y rectangulares", Activo = true },
            new { Id = 3, Nombre = "Laminado", Descripcion = "Perfiles laminados en caliente", Activo = true },
            new { Id = 4, Nombre = "Viga", Descripcion = "Vigas IPE, HEA y similares", Activo = true },
            new { Id = 5, Nombre = "Plancha", Descripcion = "Planchas lisas y diamantadas", Activo = true },
            new { Id = 6, Nombre = "Malla", Descripcion = "Mallas electrosoldadas", Activo = true },
            new { Id = 7, Nombre = "Barras", Descripcion = "Barras de construcción y lisas", Activo = true });

        mb.Entity<Producto>().HasData(
            Prod(1, 1, "ANG-20-20-2", "Ángulo 20x20x2", Producto.Tira, 7990, 40, 10),
            Prod(2, 1, "ANG-30-30-3", "Ángulo 30x30x3", Producto.Tira, 14990, 35, 10),
            Prod(3, 1, "ANG-50-50-4", "Ángulo 50x50x4", Producto.Tira, 32990, 20, 5),
            Prod(4, 2, "CUA-40-40-2", "Perfil cuadrado 40x40x2", Producto.Tira, 21990, 25, 8),
            Prod(5, 4, "VIG-IPE-100", "Viga IPE 100 x 6 m", Producto.Tira, 89990, 10, 2),
            Prod(6, 5, "PLA-LIS-2MM", "Plancha lisa 2 mm", Producto.Unidad, 59990, 12, 3),
            Prod(7, 6, "MAL-C92", "Malla C-92", Producto.Unidad, 42990, 15, 5),
            Prod(8, 7, "BAR-12-6M", "Barra estriada 12 mm x 6 m", Producto.Tira, 9990, 80, 20));
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
        Activo = true,
        FechaCreacion = FechaSemilla
    };
}
