using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lienzo.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAsistenciaEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SgaPersonaId",
                table: "usuarios",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "clases_asistencia",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActividadId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "uuid", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    HoraInicio = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    HoraFin = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    SgaComisionId = table.Column<int>(type: "integer", nullable: false),
                    SgaClaseId = table.Column<int>(type: "integer", nullable: true),
                    CheckedInAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CheckedInByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clases_asistencia", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clases_asistencia_Actividades_ActividadId",
                        column: x => x.ActividadId,
                        principalTable: "Actividades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_clases_asistencia_aulas_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "aulas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_clases_asistencia_reservas_ReservationId",
                        column: x => x.ReservationId,
                        principalTable: "reservas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "asistencias_alumnos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    SgaAlumnoId = table.Column<int>(type: "integer", nullable: false),
                    SgaPersonaId = table.Column<int>(type: "integer", nullable: false),
                    AlumnoNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AlumnoDocumento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Presente = table.Column<bool>(type: "boolean", nullable: false),
                    MarcadoPorUsuarioId = table.Column<Guid>(type: "uuid", nullable: true),
                    MarcadoAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SgaAsistenciaId = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_asistencias_alumnos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_asistencias_alumnos_clases_asistencia_ClaseId",
                        column: x => x.ClaseId,
                        principalTable: "clases_asistencia",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_asistencias_alumnos_ClaseId",
                table: "asistencias_alumnos",
                column: "ClaseId");

            migrationBuilder.CreateIndex(
                name: "IX_clases_asistencia_ActividadId",
                table: "clases_asistencia",
                column: "ActividadId");

            migrationBuilder.CreateIndex(
                name: "IX_clases_asistencia_ClassroomId",
                table: "clases_asistencia",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_clases_asistencia_ReservationId",
                table: "clases_asistencia",
                column: "ReservationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "asistencias_alumnos");

            migrationBuilder.DropTable(
                name: "clases_asistencia");

            migrationBuilder.DropColumn(
                name: "SgaPersonaId",
                table: "usuarios");
        }
    }
}
