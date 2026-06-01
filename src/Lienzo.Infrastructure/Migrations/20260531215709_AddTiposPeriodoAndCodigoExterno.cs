using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lienzo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTiposPeriodoAndCodigoExterno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actividades_Periodos_PeriodoId",
                table: "Actividades");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Periodos",
                table: "Periodos");

            migrationBuilder.RenameTable(
                name: "Periodos",
                newName: "periodos");

            migrationBuilder.RenameColumn(
                name: "Nombre",
                table: "periodos",
                newName: "nombre");

            migrationBuilder.RenameColumn(
                name: "Anio",
                table: "periodos",
                newName: "anio");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "periodos",
                newName: "actualizado_en");

            migrationBuilder.RenameColumn(
                name: "FechaInicio",
                table: "periodos",
                newName: "fecha_inicio");

            migrationBuilder.RenameColumn(
                name: "FechaFin",
                table: "periodos",
                newName: "fecha_fin");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "periodos",
                newName: "creado_en");

            migrationBuilder.AlterColumn<string>(
                name: "nombre",
                table: "periodos",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "codigo_externo",
                table: "periodos",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "tipo_periodo_id",
                table: "periodos",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Carreras",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Carreras",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "CodigoExterno",
                table: "Carreras",
                type: "integer",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Actividades",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "DiaSemana",
                table: "Actividades",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoMateria",
                table: "Actividades",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "CodigoExterno",
                table: "Actividades",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_periodos",
                table: "periodos",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "tipos_periodo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    codigo_externo = table.Column<int>(type: "integer", nullable: true),
                    creado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    actualizado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipos_periodo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_periodos_tipo_periodo_id",
                table: "periodos",
                column: "tipo_periodo_id");

            migrationBuilder.AddForeignKey(
                name: "FK_Actividades_periodos_PeriodoId",
                table: "Actividades",
                column: "PeriodoId",
                principalTable: "periodos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_periodos_tipos_periodo_tipo_periodo_id",
                table: "periodos",
                column: "tipo_periodo_id",
                principalTable: "tipos_periodo",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Actividades_periodos_PeriodoId",
                table: "Actividades");

            migrationBuilder.DropForeignKey(
                name: "FK_periodos_tipos_periodo_tipo_periodo_id",
                table: "periodos");

            migrationBuilder.DropTable(
                name: "tipos_periodo");

            migrationBuilder.DropPrimaryKey(
                name: "PK_periodos",
                table: "periodos");

            migrationBuilder.DropIndex(
                name: "IX_periodos_tipo_periodo_id",
                table: "periodos");

            migrationBuilder.DropColumn(
                name: "codigo_externo",
                table: "periodos");

            migrationBuilder.DropColumn(
                name: "tipo_periodo_id",
                table: "periodos");

            migrationBuilder.DropColumn(
                name: "CodigoExterno",
                table: "Carreras");

            migrationBuilder.DropColumn(
                name: "CodigoExterno",
                table: "Actividades");

            migrationBuilder.RenameTable(
                name: "periodos",
                newName: "Periodos");

            migrationBuilder.RenameColumn(
                name: "nombre",
                table: "Periodos",
                newName: "Nombre");

            migrationBuilder.RenameColumn(
                name: "anio",
                table: "Periodos",
                newName: "Anio");

            migrationBuilder.RenameColumn(
                name: "fecha_inicio",
                table: "Periodos",
                newName: "FechaInicio");

            migrationBuilder.RenameColumn(
                name: "fecha_fin",
                table: "Periodos",
                newName: "FechaFin");

            migrationBuilder.RenameColumn(
                name: "creado_en",
                table: "Periodos",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "actualizado_en",
                table: "Periodos",
                newName: "UpdatedAt");

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Periodos",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Carreras",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Carreras",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "Actividades",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "DiaSemana",
                table: "Actividades",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CodigoMateria",
                table: "Actividades",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Periodos",
                table: "Periodos",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Actividades_Periodos_PeriodoId",
                table: "Actividades",
                column: "PeriodoId",
                principalTable: "Periodos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
