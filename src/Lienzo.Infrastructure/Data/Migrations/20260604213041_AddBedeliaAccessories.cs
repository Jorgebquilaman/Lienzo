using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lienzo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBedeliaAccessories : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "accesorios_bedelia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    eliminado = table.Column<bool>(type: "boolean", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_accesorios_bedelia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "entrega_accesorios",
                columns: table => new
                {
                    entrega_llave_id = table.Column<Guid>(type: "uuid", nullable: false),
                    accesorio_id = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entrega_accesorios", x => new { x.entrega_llave_id, x.accesorio_id });
                    table.ForeignKey(
                        name: "FK_entrega_accesorios_accesorios_bedelia_accesorio_id",
                        column: x => x.accesorio_id,
                        principalTable: "accesorios_bedelia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_entrega_accesorios_entregas_llaves_entrega_llave_id",
                        column: x => x.entrega_llave_id,
                        principalTable: "entregas_llaves",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_entrega_accesorios_accesorio_id",
                table: "entrega_accesorios",
                column: "accesorio_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entrega_accesorios");

            migrationBuilder.DropTable(
                name: "accesorios_bedelia");
        }
    }
}
