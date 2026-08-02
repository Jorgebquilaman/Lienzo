using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lienzo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccessoryConfirmation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "confirmacion_accesorios_recibida_en",
                table: "reservas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "requiere_confirmacion_accesorios",
                table: "reservas",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "token_confirmacion_accesorios",
                table: "reservas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "aulas_accesorios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    aula_id = table.Column<Guid>(type: "uuid", nullable: false),
                    accesorio_id = table.Column<Guid>(type: "uuid", nullable: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    eliminado = table.Column<bool>(type: "boolean", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_aulas_accesorios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_aulas_accesorios_accesorios_bedelia_accesorio_id",
                        column: x => x.accesorio_id,
                        principalTable: "accesorios_bedelia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_aulas_accesorios_aulas_aula_id",
                        column: x => x.aula_id,
                        principalTable: "aulas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "reserva_accesorios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    reserva_id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    solicitado = table.Column<bool>(type: "boolean", nullable: false),
                    confirmado = table.Column<bool>(type: "boolean", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    eliminado = table.Column<bool>(type: "boolean", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_reserva_accesorios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_reserva_accesorios_reservas_reserva_id",
                        column: x => x.reserva_id,
                        principalTable: "reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_aulas_accesorios_accesorio_id",
                table: "aulas_accesorios",
                column: "accesorio_id");

            migrationBuilder.CreateIndex(
                name: "ix_aulas_accesorios_aula_accesorio",
                table: "aulas_accesorios",
                columns: new[] { "aula_id", "accesorio_id" },
                unique: true,
                filter: "\"eliminado\" = false");

            migrationBuilder.CreateIndex(
                name: "ix_reserva_accesorios_reserva_id",
                table: "reserva_accesorios",
                column: "reserva_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "aulas_accesorios");

            migrationBuilder.DropTable(
                name: "reserva_accesorios");

            migrationBuilder.DropColumn(
                name: "confirmacion_accesorios_recibida_en",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "requiere_confirmacion_accesorios",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "token_confirmacion_accesorios",
                table: "reservas");
        }
    }
}
