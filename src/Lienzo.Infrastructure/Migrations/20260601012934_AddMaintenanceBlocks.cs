using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lienzo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMaintenanceBlocks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "bloques_mantenimiento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    aula_id = table.Column<Guid>(type: "uuid", nullable: false),
                    inicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fin = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    motivo = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    creado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    activo = table.Column<bool>(type: "boolean", nullable: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    eliminado = table.Column<bool>(type: "boolean", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bloques_mantenimiento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_bloques_mantenimiento_aulas_aula_id",
                        column: x => x.aula_id,
                        principalTable: "aulas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_bloques_mantenimiento_activo",
                table: "bloques_mantenimiento",
                column: "activo");

            migrationBuilder.CreateIndex(
                name: "ix_bloques_mantenimiento_aula",
                table: "bloques_mantenimiento",
                column: "aula_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "bloques_mantenimiento");
        }
    }
}
