using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lienzo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDiasDictado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DiasDictado",
                table: "Actividades",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiasDictado",
                table: "Actividades");
        }
    }
}
