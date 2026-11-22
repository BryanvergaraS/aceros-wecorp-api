using Microsoft.EntityFrameworkCore;
using WeCorp.Catalogo.Api.Models;

namespace WeCorp.Catalogo.Api.Data;

/// <summary>Contexto de EF Core de la base WeCorpCatalogoDB (identidad y catálogo).</summary>
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Familia> Familias => Set<Familia>();
    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Rol>(e =>
        {
            e.ToTable("Roles");
            e.Property(x => x.Nombre).HasMaxLength(30).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(200);
            e.HasIndex(x => x.Nombre).IsUnique().HasDatabaseName("UQ_Roles_Nombre");
        });

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

        modelBuilder.Entity<Familia>(e =>
        {
            e.ToTable("Familias");
            e.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(200);
            e.HasIndex(x => x.Nombre).IsUnique().HasDatabaseName("UQ_Familias_Nombre");
        });

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
            // Control de concurrencia optimista: si dos ventas cambian el mismo stock a la vez,
            // la segunda falla (DbUpdateConcurrencyException → 409) en vez de pisar a la primera.
            e.Property(x => x.Stock).IsConcurrencyToken();
            e.HasIndex(x => x.Sku).IsUnique().HasDatabaseName("UQ_Productos_Sku");
            e.HasIndex(x => x.FamiliaId).HasDatabaseName("IX_Productos_FamiliaId");
            e.HasOne(x => x.Familia).WithMany(f => f.Productos).HasForeignKey(x => x.FamiliaId)
                .HasConstraintName("FK_Productos_Familias").OnDelete(DeleteBehavior.Restrict);
        });

        SeedData.Aplicar(modelBuilder);
    }
}
