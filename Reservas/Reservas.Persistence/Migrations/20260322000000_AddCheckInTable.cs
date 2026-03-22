using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckInTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheckIns",
                columns: table => new
                {
                    CheckInId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReservaId = table.Column<int>(type: "int", nullable: false),
                    NombreCompleto = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    FechaCheckIn = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    DireccionIp = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    Estado = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "Completado")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheckIns", x => x.CheckInId);
                    table.ForeignKey(
                        name: "FK_CheckIns_Reservas_ReservaId",
                        column: x => x.ReservaId,
                        principalTable: "Reservas",
                        principalColumn: "ReservaId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_Email",
                table: "CheckIns",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_CheckIns_ReservaId",
                table: "CheckIns",
                column: "ReservaId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheckIns");
        }
    }
}
