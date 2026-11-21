using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Granja.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Granjas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    NumeroVacas = table.Column<int>(type: "INTEGER", nullable: false),
                    LitrosPorVaca = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Granjas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Lotes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    FechaProduccion = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    FechaVencimiento = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    LitrosUsados = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    CantidadInicial = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    CantidadDisponible = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    CantidadDanada = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lotes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ventas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    PrecioUnitario = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false),
                    Cantidad = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    Total = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ventas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ParametrosProducto",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GranjaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    Rendimiento = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false),
                    VidaUtilDias = table.Column<int>(type: "INTEGER", nullable: false),
                    Precio = table.Column<decimal>(type: "TEXT", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParametrosProducto", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParametrosProducto_Granjas_GranjaId",
                        column: x => x.GranjaId,
                        principalTable: "Granjas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vacas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    GranjaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vacas_Granjas_GranjaId",
                        column: x => x.GranjaId,
                        principalTable: "Granjas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VentaDetalles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    VentaId = table.Column<int>(type: "INTEGER", nullable: false),
                    LoteId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaProduccion = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    Cantidad = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VentaDetalles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Lotes_LoteId",
                        column: x => x.LoteId,
                        principalTable: "Lotes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VentaDetalles_Ventas_VentaId",
                        column: x => x.VentaId,
                        principalTable: "Ventas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Producciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fecha = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    VacaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Litros = table.Column<decimal>(type: "TEXT", precision: 18, scale: 4, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Producciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Producciones_Vacas_VacaId",
                        column: x => x.VacaId,
                        principalTable: "Vacas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Granjas",
                columns: new[] { "Id", "LitrosPorVaca", "Nombre", "NumeroVacas" },
                values: new object[] { 1, 15m, "Granja Lechera", 10 });

            migrationBuilder.InsertData(
                table: "ParametrosProducto",
                columns: new[] { "Id", "GranjaId", "Precio", "Rendimiento", "Tipo", "VidaUtilDias" },
                values: new object[,]
                {
                    { 1, 1, 28000m, 10m, "Queso", 30 },
                    { 2, 1, 6000m, 1m, "Yogurt", 15 }
                });

            migrationBuilder.InsertData(
                table: "Vacas",
                columns: new[] { "Id", "Activa", "GranjaId", "Nombre" },
                values: new object[,]
                {
                    { 1, true, 1, "Vaca 01" },
                    { 2, true, 1, "Vaca 02" },
                    { 3, true, 1, "Vaca 03" },
                    { 4, true, 1, "Vaca 04" },
                    { 5, true, 1, "Vaca 05" },
                    { 6, true, 1, "Vaca 06" },
                    { 7, true, 1, "Vaca 07" },
                    { 8, true, 1, "Vaca 08" },
                    { 9, true, 1, "Vaca 09" },
                    { 10, true, 1, "Vaca 10" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lotes_Tipo_FechaProduccion",
                table: "Lotes",
                columns: new[] { "Tipo", "FechaProduccion" });

            migrationBuilder.CreateIndex(
                name: "IX_ParametrosProducto_GranjaId_Tipo",
                table: "ParametrosProducto",
                columns: new[] { "GranjaId", "Tipo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Producciones_Fecha_VacaId",
                table: "Producciones",
                columns: new[] { "Fecha", "VacaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Producciones_VacaId",
                table: "Producciones",
                column: "VacaId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacas_GranjaId",
                table: "Vacas",
                column: "GranjaId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_LoteId",
                table: "VentaDetalles",
                column: "LoteId");

            migrationBuilder.CreateIndex(
                name: "IX_VentaDetalles_VentaId",
                table: "VentaDetalles",
                column: "VentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Ventas_Fecha",
                table: "Ventas",
                column: "Fecha");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParametrosProducto");

            migrationBuilder.DropTable(
                name: "Producciones");

            migrationBuilder.DropTable(
                name: "VentaDetalles");

            migrationBuilder.DropTable(
                name: "Vacas");

            migrationBuilder.DropTable(
                name: "Lotes");

            migrationBuilder.DropTable(
                name: "Ventas");

            migrationBuilder.DropTable(
                name: "Granjas");
        }
    }
}
