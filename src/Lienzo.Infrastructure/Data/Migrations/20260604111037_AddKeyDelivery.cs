using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lienzo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKeyDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "entregas_llaves",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    aula_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entregado_a_usuario_id = table.Column<Guid>(type: "uuid", nullable: true),
                    entregado_a_nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    entregado_por_usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    entregado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    devuelto_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    eliminado = table.Column<bool>(type: "boolean", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entregas_llaves", x => x.Id);
                    table.ForeignKey(
                        name: "FK_entregas_llaves_aulas_aula_id",
                        column: x => x.aula_id,
                        principalTable: "aulas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_entregas_llaves_aula",
                table: "entregas_llaves",
                column: "aula_id");

            migrationBuilder.CreateIndex(
                name: "ix_entregas_llaves_devuelto",
                table: "entregas_llaves",
                column: "devuelto_en");

            migrationBuilder.CreateIndex(
                name: "ix_entregas_llaves_entregado_a",
                table: "entregas_llaves",
                column: "entregado_a_usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entregas_llaves");
        }
    }
}
