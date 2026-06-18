using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ControlTaxi.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RelacionesTicketTaxista : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RelacionesTicketTaxista",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FolioApp = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    FolioOperacion = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    FolioPos = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Hotel = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Origen = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Destino = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Nacionalidad = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Pax = table.Column<int>(type: "int", nullable: false),
                    TaxistaId = table.Column<int>(type: "int", nullable: true),
                    TaxistaNombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    TransporteTipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Gafete = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Placas = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Observaciones = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Venta = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Dejada = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DejadaPagada = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Comision = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Pago = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FechaPagoDejada = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UsuarioPagoDejada = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TicketPagoDejada = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RelacionesTicketTaxista", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Taxistas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clave = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Unidad = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Placas = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Estatus = table.Column<int>(type: "int", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Taxistas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RelacionesTicketTaxista_FolioApp",
                table: "RelacionesTicketTaxista",
                column: "FolioApp",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RelacionesTicketTaxista_FolioOperacion",
                table: "RelacionesTicketTaxista",
                column: "FolioOperacion");

            migrationBuilder.CreateIndex(
                name: "IX_RelacionesTicketTaxista_TaxistaId",
                table: "RelacionesTicketTaxista",
                column: "TaxistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Taxistas_Clave",
                table: "Taxistas",
                column: "Clave",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RelacionesTicketTaxista");

            migrationBuilder.DropTable(
                name: "Taxistas");
        }
    }
}
