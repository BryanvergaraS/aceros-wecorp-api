using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AcerosWeCorp.Api.Migrations
{
    /// <inheritdoc />
    public partial class InicialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Familias",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Familias", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Proveedores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rut = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    RazonSocial = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Proveedores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Productos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FamiliaId = table.Column<int>(type: "int", nullable: false),
                    Sku = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UnidadVenta = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    PesoKg = table.Column<decimal>(type: "decimal(10,3)", precision: 10, scale: 3, nullable: true),
                    PrecioVenta = table.Column<decimal>(type: "decimal(12,0)", precision: 12, scale: 0, nullable: false),
                    Stock = table.Column<int>(type: "int", nullable: false),
                    StockMinimo = table.Column<int>(type: "int", nullable: false),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Productos", x => x.Id);
                    table.CheckConstraint("CK_Productos_PrecioVenta", "[PrecioVenta] > 0");
                    table.CheckConstraint("CK_Productos_Stock", "[Stock] >= 0");
                    table.CheckConstraint("CK_Productos_StockMinimo", "[StockMinimo] >= 0");
                    table.CheckConstraint("CK_Productos_UnidadVenta", "[UnidadVenta] IN ('Tira', 'Unidad')");
                    table.ForeignKey(
                        name: "FK_Productos_Familias",
                        column: x => x.FamiliaId,
                        principalTable: "Familias",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RolId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Estado = table.Column<bool>(type: "bit", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Roles",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ProductoProveedores",
                columns: table => new
                {
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    ProveedorId = table.Column<int>(type: "int", nullable: false),
                    CostoCompra = table.Column<decimal>(type: "decimal(12,2)", precision: 12, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductoProveedores", x => new { x.ProductoId, x.ProveedorId });
                    table.CheckConstraint("CK_ProductoProveedores_Costo", "[CostoCompra] IS NULL OR [CostoCompra] >= 0");
                    table.ForeignKey(
                        name: "FK_ProductoProveedores_Productos",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductoProveedores_Proveedores",
                        column: x => x.ProveedorId,
                        principalTable: "Proveedores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LogsAuditoria",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    MetodoHttp = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    RutaEndpoint = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    CodigoEstadoHttp = table.Column<int>(type: "int", nullable: false),
                    IpOrigen = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    TiempoRespuestaMs = table.Column<int>(type: "int", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAuditoria", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LogsAuditoria_Usuarios",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "NotasVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Folio = table.Column<int>(type: "int", nullable: false),
                    ClienteNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ClienteRut = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    Total = table.Column<decimal>(type: "decimal(12,0)", precision: 12, scale: 0, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasVenta", x => x.Id);
                    table.CheckConstraint("CK_NotasVenta_Estado", "[Estado] IN ('Pendiente', 'Pagada', 'Anulada')");
                    table.CheckConstraint("CK_NotasVenta_Total", "[Total] >= 0");
                    table.ForeignKey(
                        name: "FK_NotasVenta_Usuarios",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DetalleNotasVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NotaVentaId = table.Column<int>(type: "int", nullable: false),
                    ProductoId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "decimal(12,0)", precision: 12, scale: 0, nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(23,0)", precision: 23, scale: 0, nullable: false, computedColumnSql: "[Cantidad] * [PrecioUnitario]", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DetalleNotasVenta", x => x.Id);
                    table.CheckConstraint("CK_DetalleNotasVenta_Cantidad", "[Cantidad] > 0");
                    table.CheckConstraint("CK_DetalleNotasVenta_Precio", "[PrecioUnitario] > 0");
                    table.ForeignKey(
                        name: "FK_DetalleNotasVenta_NotasVenta",
                        column: x => x.NotaVentaId,
                        principalTable: "NotasVenta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DetalleNotasVenta_Productos",
                        column: x => x.ProductoId,
                        principalTable: "Productos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Familias",
                columns: new[] { "Id", "Descripcion", "Estado", "Nombre" },
                values: new object[,]
                {
                    { 1, "Perfiles abiertos: ángulos, canales, tees", true, "Abierto" },
                    { 2, "Perfiles cerrados: cuadrados y rectangulares", true, "Cerrado" },
                    { 3, "Perfiles laminados en caliente", true, "Laminado" },
                    { 4, "Vigas IPE, HEA y similares", true, "Viga" },
                    { 5, "Planchas lisas y diamantadas", true, "Plancha" },
                    { 6, "Mallas electrosoldadas", true, "Malla" },
                    { 7, "Barras de construcción y lisas", true, "Barras" }
                });

            migrationBuilder.InsertData(
                table: "Proveedores",
                columns: new[] { "Id", "Direccion", "Email", "Estado", "RazonSocial", "Rut", "Telefono" },
                values: new object[,]
                {
                    { 1, null, "ventas@proveedor1.example", true, "Aceros del Sur SpA (ejemplo)", "76.111.111-1", "+56 9 1111 1111" },
                    { 2, null, "contacto@proveedor2.example", true, "Perfiles Biobío Ltda. (ejemplo)", "76.222.222-2", "+56 9 2222 2222" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Acceso total: gestiona catálogo, precios, proveedores y usuarios", "Administrador" },
                    { 2, "Consulta el catálogo y emite notas de venta", "Vendedor" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Descripcion", "Estado", "FamiliaId", "FechaCreacion", "Nombre", "PesoKg", "PrecioVenta", "Sku", "Stock", "StockMinimo", "UnidadVenta" },
                values: new object[,]
                {
                    { 1, null, true, 1, new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ángulo 20x20x2", null, 7990m, "ANG-20-20-2", 40, 10, "Tira" },
                    { 2, null, true, 1, new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ángulo 30x30x3", null, 14990m, "ANG-30-30-3", 35, 10, "Tira" },
                    { 3, null, true, 1, new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ángulo 50x50x4", null, 32990m, "ANG-50-50-4", 20, 5, "Tira" },
                    { 4, null, true, 2, new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Perfil cuadrado 40x40x2", null, 21990m, "CUA-40-40-2", 25, 8, "Tira" },
                    { 5, null, true, 5, new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Plancha lisa 2 mm", null, 59990m, "PLA-LIS-2MM", 12, 3, "Unidad" },
                    { 6, null, true, 6, new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Malla C-92", null, 42990m, "MAL-C92", 15, 5, "Unidad" },
                    { 7, null, true, 7, new DateTime(2026, 10, 19, 0, 0, 0, 0, DateTimeKind.Unspecified), "Barra estriada 12 mm x 6 m", null, 9990m, "BAR-12-6M", 80, 20, "Tira" }
                });

            migrationBuilder.InsertData(
                table: "ProductoProveedores",
                columns: new[] { "ProductoId", "ProveedorId", "CostoCompra" },
                values: new object[,]
                {
                    { 1, 1, 5200m },
                    { 2, 1, 9800m },
                    { 3, 2, 21500m },
                    { 4, 2, 14300m },
                    { 5, 1, 39000m },
                    { 6, 2, 28000m },
                    { 7, 1, 6500m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_DetalleNotasVenta_NotaVentaId",
                table: "DetalleNotasVenta",
                column: "NotaVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleNotasVenta_ProductoId",
                table: "DetalleNotasVenta",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "UQ_Familias_Nombre",
                table: "Familias",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LogsAuditoria_UsuarioId",
                table: "LogsAuditoria",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasVenta_UsuarioId",
                table: "NotasVenta",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "UQ_NotasVenta_Folio",
                table: "NotasVenta",
                column: "Folio",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ProductoProveedores_ProveedorId",
                table: "ProductoProveedores",
                column: "ProveedorId");

            migrationBuilder.CreateIndex(
                name: "IX_Productos_FamiliaId",
                table: "Productos",
                column: "FamiliaId");

            migrationBuilder.CreateIndex(
                name: "UQ_Productos_Sku",
                table: "Productos",
                column: "Sku",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Proveedores_Rut",
                table: "Proveedores",
                column: "Rut",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UQ_Roles_Nombre",
                table: "Roles",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_RolId",
                table: "Usuarios",
                column: "RolId");

            migrationBuilder.CreateIndex(
                name: "UQ_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleNotasVenta");

            migrationBuilder.DropTable(
                name: "LogsAuditoria");

            migrationBuilder.DropTable(
                name: "ProductoProveedores");

            migrationBuilder.DropTable(
                name: "NotasVenta");

            migrationBuilder.DropTable(
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Proveedores");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Familias");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
