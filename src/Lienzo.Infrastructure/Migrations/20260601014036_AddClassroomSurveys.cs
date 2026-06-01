using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lienzo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClassroomSurveys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "encuestas_aula",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    reserva_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    calificacion_condicion = table.Column<decimal>(type: "numeric(2,1)", precision: 2, scale: 1, nullable: false),
                    calificacion_equipamiento = table.Column<decimal>(type: "numeric(2,1)", precision: 2, scale: 1, nullable: false),
                    calificacion_limpieza = table.Column<decimal>(type: "numeric(2,1)", precision: 2, scale: 1, nullable: false),
                    calificacion_general = table.Column<decimal>(type: "numeric(2,1)", precision: 2, scale: 1, nullable: false),
                    comentario = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    eliminado = table.Column<bool>(type: "boolean", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_encuestas_aula", x => x.Id);
                    table.ForeignKey(
                        name: "FK_encuestas_aula_reservas_reserva_id",
                        column: x => x.reserva_id,
                        principalTable: "reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "ix_encuestas_aula_reserva",
                table: "encuestas_aula",
                column: "reserva_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_encuestas_aula_usuario",
                table: "encuestas_aula",
                column: "usuario_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "encuestas_aula");
        }
    }
}
