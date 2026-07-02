using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lienzo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFloorPlanAndMapPositions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "url_plano",
                table: "edificios",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "posicion_x",
                table: "aulas",
                type: "real",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "posicion_y",
                table: "aulas",
                type: "real",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "url_plano",
                table: "edificios");

            migrationBuilder.DropColumn(
                name: "posicion_x",
                table: "aulas");

            migrationBuilder.DropColumn(
                name: "posicion_y",
                table: "aulas");
        }
    }
}
