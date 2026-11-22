using Microsoft.EntityFrameworkCore;
using WeCorp.Ventas.Api.Models;

namespace WeCorp.Ventas.Api.Data;

/// <summary>Contexto de EF Core de la base WeCorpVentasDB.</summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<NotaVenta> NotasVenta => Set<NotaVenta>();
    public DbSet<DetalleNotaVenta> DetalleNotasVenta => Set<DetalleNotaVenta>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Clientes
        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("Clientes");
            e.Property(x => x.Rut).HasMaxLength(12).IsRequired();
            e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Telefono).HasMaxLength(20);
            e.Property(x => x.Direccion).HasMaxLength(200);
            e.Property(x => x.FechaRegistro).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.Rut).IsUnique().HasDatabaseName("UQ_Clientes_Rut");
            e.HasIndex(x => x.Nombre).HasDatabaseName("IX_Clientes_Nombre");
        });

        // 2. NotasVenta
        modelBuilder.Entity<NotaVenta>(e =>
        {
            e.ToTable("NotasVenta", t =>
            {
                t.HasCheckConstraint("CK_NotasVenta_Estado", "[Estado] IN ('Pendiente', 'Pagada', 'Anulada')");
                t.HasCheckConstraint("CK_NotasVenta_Total", "[Total] >= 0");
            });
            e.Property(x => x.Folio).HasMaxLength(20).IsRequired();
            e.Property(x => x.VendedorNombre).HasMaxLength(120).IsRequired();
            e.Property(x => x.Estado).HasMaxLength(15).IsRequired();
            e.Property(x => x.Total).HasPrecision(12, 0);
            e.Property(x => x.Observaciones).HasMaxLength(300);
            e.Property(x => x.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.Folio).IsUnique().HasDatabaseName("UQ_NotasVenta_Folio");
            e.HasIndex(x => x.ClienteId).HasDatabaseName("IX_NotasVenta_ClienteId");
            e.HasIndex(x => x.VendedorId).HasDatabaseName("IX_NotasVenta_VendedorId");
            e.HasOne(x => x.Cliente).WithMany(c => c.NotasVenta).HasForeignKey(x => x.ClienteId)
                .HasConstraintName("FK_NotasVenta_Clientes").OnDelete(DeleteBehavior.Restrict);
        });

        // 3. DetalleNotasVenta
        modelBuilder.Entity<DetalleNotaVenta>(e =>
        {
            e.ToTable("DetalleNotasVenta", t =>
            {
                t.HasCheckConstraint("CK_DetalleNotasVenta_Cantidad", "[Cantidad] > 0");
                t.HasCheckConstraint("CK_DetalleNotasVenta_Precio", "[PrecioUnitario] > 0");
            });
            e.Property(x => x.Sku).HasMaxLength(50).IsRequired();
            e.Property(x => x.NombreProducto).HasMaxLength(150).IsRequired();
            e.Property(x => x.PrecioUnitario).HasPrecision(12, 0);
            e.Property(x => x.Subtotal).HasPrecision(23, 0)
                .HasComputedColumnSql("[Cantidad] * [PrecioUnitario]", stored: true);
            e.HasIndex(x => x.NotaVentaId).HasDatabaseName("IX_DetalleNotasVenta_NotaVentaId");
            e.HasIndex(x => x.ProductoId).HasDatabaseName("IX_DetalleNotasVenta_ProductoId");
            e.HasOne(x => x.NotaVenta).WithMany(n => n.Detalles).HasForeignKey(x => x.NotaVentaId)
                .HasConstraintName("FK_DetalleNotasVenta_NotasVenta").OnDelete(DeleteBehavior.Cascade);
        });

        SeedData.Aplicar(modelBuilder);
    }
}
