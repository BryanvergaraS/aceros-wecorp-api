using AcerosWeCorp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace AcerosWeCorp.Api.Data;

/// <summary>
/// Contexto de EF Core. La configuración replica el script database/AcerosWeCorpDB.sql:
/// mismos nombres de tablas, largos, claves, únicos, CHECK y relaciones.
/// </summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Familia> Familias => Set<Familia>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<ProductoProveedor> ProductoProveedores => Set<ProductoProveedor>();
    public DbSet<NotaVenta> NotasVenta => Set<NotaVenta>();
    public DbSet<DetalleNotaVenta> DetalleNotasVenta => Set<DetalleNotaVenta>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 1. Roles
        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("Roles");
            e.Property(x => x.Nombre).HasMaxLength(30).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(200);
            e.HasIndex(x => x.Nombre).IsUnique().HasDatabaseName("UQ_Roles_Nombre");
        });

        // 2. Usuarios
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("Usuarios");
            e.Property(x => x.Nombre).HasMaxLength(60).IsRequired();
            e.Property(x => x.Apellido).HasMaxLength(60).IsRequired();
            e.Property(x => x.Email).HasMaxLength(150).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
            e.Property(x => x.Telefono).HasMaxLength(20);
            e.Property(x => x.FechaRegistro).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.Email).IsUnique().HasDatabaseName("UQ_Usuarios_Email");
            e.HasOne(x => x.Rol).WithMany(r => r.Usuarios).HasForeignKey(x => x.RolId)
                .HasConstraintName("FK_Usuarios_Roles").OnDelete(DeleteBehavior.Restrict);
        });

        // 3. Familias
        modelBuilder.Entity<Familia>(e =>
        {
            e.ToTable("Familias");
            e.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(200);
            e.HasIndex(x => x.Nombre).IsUnique().HasDatabaseName("UQ_Familias_Nombre");
        });

        // 4. Productos
        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("Productos", t =>
            {
                t.HasCheckConstraint("CK_Productos_UnidadVenta", "[UnidadVenta] IN ('Tira', 'Unidad')");
                t.HasCheckConstraint("CK_Productos_PrecioVenta", "[PrecioVenta] > 0");
                t.HasCheckConstraint("CK_Productos_Stock", "[Stock] >= 0");
                t.HasCheckConstraint("CK_Productos_StockMinimo", "[StockMinimo] >= 0");
            });
            e.Property(x => x.Sku).HasMaxLength(50).IsRequired();
            e.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(300);
            e.Property(x => x.UnidadVenta).HasMaxLength(10).IsRequired();
            e.Property(x => x.PesoKg).HasPrecision(10, 3);
            e.Property(x => x.PrecioVenta).HasPrecision(12, 0);
            e.Property(x => x.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.Sku).IsUnique().HasDatabaseName("UQ_Productos_Sku");
            e.HasIndex(x => x.FamiliaId).HasDatabaseName("IX_Productos_FamiliaId");
            e.HasOne(x => x.Familia).WithMany(f => f.Productos).HasForeignKey(x => x.FamiliaId)
                .HasConstraintName("FK_Productos_Familias").OnDelete(DeleteBehavior.Restrict);
        });

        // 5. Proveedores
        modelBuilder.Entity<Proveedor>(e =>
        {
            e.ToTable("Proveedores");
            e.Property(x => x.Rut).HasMaxLength(12).IsRequired();
            e.Property(x => x.RazonSocial).HasMaxLength(150).IsRequired();
            e.Property(x => x.Telefono).HasMaxLength(20);
            e.Property(x => x.Email).HasMaxLength(150);
            e.Property(x => x.Direccion).HasMaxLength(200);
            e.HasIndex(x => x.Rut).IsUnique().HasDatabaseName("UQ_Proveedores_Rut");
        });

        // 6. ProductoProveedores (N a N)
        modelBuilder.Entity<ProductoProveedor>(e =>
        {
            e.ToTable("ProductoProveedores", t =>
                t.HasCheckConstraint("CK_ProductoProveedores_Costo", "[CostoCompra] IS NULL OR [CostoCompra] >= 0"));
            e.HasKey(x => new { x.ProductoId, x.ProveedorId });
            e.Property(x => x.CostoCompra).HasPrecision(12, 2);
            e.HasIndex(x => x.ProveedorId).HasDatabaseName("IX_ProductoProveedores_ProveedorId");
            e.HasOne(x => x.Producto).WithMany(p => p.ProductoProveedores).HasForeignKey(x => x.ProductoId)
                .HasConstraintName("FK_ProductoProveedores_Productos").OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Proveedor).WithMany(p => p.ProductoProveedores).HasForeignKey(x => x.ProveedorId)
                .HasConstraintName("FK_ProductoProveedores_Proveedores").OnDelete(DeleteBehavior.Cascade);
        });

        // 7. NotasVenta
        modelBuilder.Entity<NotaVenta>(e =>
        {
            e.ToTable("NotasVenta", t =>
            {
                t.HasCheckConstraint("CK_NotasVenta_Estado", "[Estado] IN ('Pendiente', 'Pagada', 'Anulada')");
                t.HasCheckConstraint("CK_NotasVenta_Total", "[Total] >= 0");
            });
            e.Property(x => x.ClienteNombre).HasMaxLength(150).IsRequired();
            e.Property(x => x.ClienteRut).HasMaxLength(12);
            e.Property(x => x.Total).HasPrecision(12, 0);
            e.Property(x => x.Estado).HasMaxLength(15).IsRequired();
            e.Property(x => x.Observaciones).HasMaxLength(300);
            e.Property(x => x.FechaCreacion).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.Folio).IsUnique().HasDatabaseName("UQ_NotasVenta_Folio");
            e.HasIndex(x => x.UsuarioId).HasDatabaseName("IX_NotasVenta_UsuarioId");
            e.HasOne(x => x.Usuario).WithMany(u => u.NotasVenta).HasForeignKey(x => x.UsuarioId)
                .HasConstraintName("FK_NotasVenta_Usuarios").OnDelete(DeleteBehavior.Restrict);
        });

        // 8. DetalleNotasVenta
        modelBuilder.Entity<DetalleNotaVenta>(e =>
        {
            e.ToTable("DetalleNotasVenta", t =>
            {
                t.HasCheckConstraint("CK_DetalleNotasVenta_Cantidad", "[Cantidad] > 0");
                t.HasCheckConstraint("CK_DetalleNotasVenta_Precio", "[PrecioUnitario] > 0");
            });
            e.Property(x => x.PrecioUnitario).HasPrecision(12, 0);
            e.Property(x => x.Subtotal).HasPrecision(23, 0)
                .HasComputedColumnSql("[Cantidad] * [PrecioUnitario]", stored: true);
            e.HasIndex(x => x.NotaVentaId).HasDatabaseName("IX_DetalleNotasVenta_NotaVentaId");
            e.HasIndex(x => x.ProductoId).HasDatabaseName("IX_DetalleNotasVenta_ProductoId");
            e.HasOne(x => x.NotaVenta).WithMany(n => n.Detalles).HasForeignKey(x => x.NotaVentaId)
                .HasConstraintName("FK_DetalleNotasVenta_NotasVenta").OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Producto).WithMany().HasForeignKey(x => x.ProductoId)
                .HasConstraintName("FK_DetalleNotasVenta_Productos").OnDelete(DeleteBehavior.Restrict);
        });

        // 9. LogsAuditoria
        modelBuilder.Entity<LogAuditoria>(e =>
        {
            e.ToTable("LogsAuditoria");
            e.Property(x => x.MetodoHttp).HasMaxLength(10).IsRequired();
            e.Property(x => x.RutaEndpoint).HasMaxLength(300).IsRequired();
            e.Property(x => x.IpOrigen).HasMaxLength(45);
            e.Property(x => x.FechaRegistro).HasDefaultValueSql("SYSDATETIME()");
            e.HasIndex(x => x.UsuarioId).HasDatabaseName("IX_LogsAuditoria_UsuarioId");
            e.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.UsuarioId)
                .HasConstraintName("FK_LogsAuditoria_Usuarios").OnDelete(DeleteBehavior.Restrict);
        });

        SeedData.Aplicar(modelBuilder);
    }
}
