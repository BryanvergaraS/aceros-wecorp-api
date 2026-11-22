using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace WeCorp.Ventas.Api.Migrations
{
    /// <inheritdoc />
    public partial class InicialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Rut = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Direccion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotasVenta",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Folio = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ClienteId = table.Column<int>(type: "int", nullable: false),
                    VendedorId = table.Column<int>(type: "int", nullable: false),
                    VendedorNombre = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Total = table.Column<decimal>(type: "decimal(12,0)", precision: 12, scale: 0, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSDATETIME()"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotasVenta", x => x.Id);
                    table.CheckConstraint("CK_NotasVenta_Estado", "[Estado] IN ('Pendiente', 'Pagada', 'Anulada')");
                    table.CheckConstraint("CK_NotasVenta_Total", "[Total] >= 0");
                    table.ForeignKey(
                        name: "FK_NotasVenta_Clientes",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
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
                    Sku = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NombreProducto = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
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
                });

            migrationBuilder.InsertData(
                table: "Clientes",
                columns: new[] { "Id", "Direccion", "Email", "FechaRegistro", "Nombre", "Rut", "Telefono" },
                values: new object[,]
                {
                    { 1, "Nacimiento, Biobío", "compras@cliente1.example", new DateTime(2026, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Constructora Los Robles Ltda. (ejemplo)", "11111111-1", "+56 9 3333 3333" },
                    { 2, "Los Ángeles, Biobío", null, new DateTime(2026, 11, 9, 0, 0, 0, 0, DateTimeKind.Unspecified), "Juan Pérez (ejemplo)", "22222222-2", "+56 9 4444 4444" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_Nombre",
                table: "Clientes",
                column: "Nombre");

            migrationBuilder.CreateIndex(
                name: "UQ_Clientes_Rut",
                table: "Clientes",
                column: "Rut",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DetalleNotasVenta_NotaVentaId",
                table: "DetalleNotasVenta",
                column: "NotaVentaId");

            migrationBuilder.CreateIndex(
                name: "IX_DetalleNotasVenta_ProductoId",
                table: "DetalleNotasVenta",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasVenta_ClienteId",
                table: "NotasVenta",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_NotasVenta_VendedorId",
                table: "NotasVenta",
                column: "VendedorId");

            migrationBuilder.CreateIndex(
                name: "UQ_NotasVenta_Folio",
                table: "NotasVenta",
                column: "Folio",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DetalleNotasVenta");

            migrationBuilder.DropTable(
                name: "NotasVenta");

            migrationBuilder.DropTable(
                name: "Clientes");
        }
    }
}
