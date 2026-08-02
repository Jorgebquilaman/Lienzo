using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Lienzo.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSourceEmailAndProcessedEmails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email_evidencia_path",
                table: "reservas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_origen_asunto",
                table: "reservas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "email_origen_fecha",
                table: "reservas",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_origen_remitente",
                table: "reservas",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "email_origen_uid",
                table: "reservas",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "processed_emails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    email_uid = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    reserva_id = table.Column<Guid>(type: "uuid", nullable: false),
                    procesado_por = table.Column<Guid>(type: "uuid", nullable: false),
                    procesado_en = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_emails", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_processed_emails_email_uid",
                table: "processed_emails",
                column: "email_uid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_processed_emails_reserva_id",
                table: "processed_emails",
                column: "reserva_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_emails");

            migrationBuilder.DropColumn(
                name: "email_evidencia_path",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "email_origen_asunto",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "email_origen_fecha",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "email_origen_remitente",
                table: "reservas");

            migrationBuilder.DropColumn(
                name: "email_origen_uid",
                table: "reservas");
        }
    }
}
