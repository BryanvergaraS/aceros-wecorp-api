using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WeCorp.Catalogo.Api.Migrations
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
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Familias", x => x.Id);
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
                    Activo = table.Column<bool>(type: "bit", nullable: false),
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
                    Activo = table.Column<bool>(type: "bit", nullable: false),
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

            migrationBuilder.InsertData(
                table: "Familias",
                columns: new[] { "Id", "Activo", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, true, "Perfiles abiertos: ángulos, canales, tees", "Abierto" },
                    { 2, true, "Perfiles cerrados: cuadrados y rectangulares", "Cerrado" },
                    { 3, true, "Perfiles laminados en caliente", "Laminado" },
                    { 4, true, "Vigas IPE, HEA y similares", "Viga" },
                    { 5, true, "Planchas lisas y diamantadas", "Plancha" },
                    { 6, true, "Mallas electrosoldadas", "Malla" },
                    { 7, true, "Barras de construcción y lisas", "Barras" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "Gestiona catálogo, usuarios y estado de las notas de venta", "Administrador" },
                    { 2, "Consulta el catálogo, registra clientes y emite notas de venta", "Vendedor" }
                });

            migrationBuilder.InsertData(
                table: "Productos",
                columns: new[] { "Id", "Activo", "Descripcion", "FamiliaId", "FechaCreacion", "Nombre", "PesoKg", "PrecioVenta", "Sku", "Stock", "StockMinimo", "UnidadVenta" },
                values: new object[,]
                {
                    { 1, true, null, 1, new DateTime(2026, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ángulo 20x20x2", null, 7990m, "ANG-20-20-2", 40, 10, "Tira" },
                    { 2, true, null, 1, new DateTime(2026, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ángulo 30x30x3", null, 14990m, "ANG-30-30-3", 35, 10, "Tira" },
                    { 3, true, null, 1, new DateTime(2026, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ángulo 50x50x4", null, 32990m, "ANG-50-50-4", 20, 5, "Tira" },
                    { 4, true, null, 2, new DateTime(2026, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Perfil cuadrado 40x40x2", null, 21990m, "CUA-40-40-2", 25, 8, "Tira" },
                    { 5, true, null, 4, new DateTime(2026, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Viga IPE 100 x 6 m", null, 89990m, "VIG-IPE-100", 10, 2, "Tira" },
                    { 6, true, null, 5, new DateTime(2026, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Plancha lisa 2 mm", null, 59990m, "PLA-LIS-2MM", 12, 3, "Unidad" },
                    { 7, true, null, 6, new DateTime(2026, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Malla C-92", null, 42990m, "MAL-C92", 15, 5, "Unidad" },
                    { 8, true, null, 7, new DateTime(2026, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Barra estriada 12 mm x 6 m", null, 9990m, "BAR-12-6M", 80, 20, "Tira" }
                });

            migrationBuilder.CreateIndex(
                name: "UQ_Familias_Nombre",
                table: "Familias",
                column: "Nombre",
                unique: true);

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
                name: "Productos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Familias");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
