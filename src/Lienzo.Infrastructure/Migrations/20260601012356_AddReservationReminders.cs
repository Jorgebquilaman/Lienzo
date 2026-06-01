using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lienzo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationReminders : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "recordatorios_reserva",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    reserva_id = table.Column<Guid>(type: "uuid", nullable: false),
                    usuario_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tipo_recordatorio = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    enviado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    eliminado = table.Column<bool>(type: "boolean", nullable: false),
                    eliminado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_recordatorios_reserva", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_recordatorios_reserva_enviado_en",
                table: "recordatorios_reserva",
                column: "enviado_en");

            migrationBuilder.CreateIndex(
                name: "ix_recordatorios_reserva_tipo",
                table: "recordatorios_reserva",
                columns: new[] { "reserva_id", "tipo_recordatorio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "recordatorios_reserva");
        }
    }
}
