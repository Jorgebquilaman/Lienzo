using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lienzo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringReservations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "grupo_recurrente_id",
                table: "reservas",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "regla_recurrencia",
                table: "reservas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "grupo_recurrente_id",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "regla_recurrencia",
                table: "reservas");
        }
    }
}
