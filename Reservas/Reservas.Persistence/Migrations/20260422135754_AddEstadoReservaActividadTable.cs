using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadoReservaActividadTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EstadosReservaActividad",
                columns: table => new
                {
                    EstadoReservaActividadId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosReservaActividad", x => x.EstadoReservaActividadId);
                });

            migrationBuilder.AddColumn<int>(
                name: "EstadoReservaActividadId",
                table: "ReservasActividades",
                type: "int",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.CreateIndex(
                name: "IX_ReservasActividades_EstadoReservaActividadId",
                table: "ReservasActividades",
                column: "EstadoReservaActividadId");

            migrationBuilder.AddForeignKey(
                name: "FK_ReservasActividades_EstadosReservaActividad",
                table: "ReservasActividades",
                column: "EstadoReservaActividadId",
                principalTable: "EstadosReservaActividad",
                principalColumn: "EstadoReservaActividadId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ReservasActividades_EstadosReservaActividad",
                table: "ReservasActividades");

            migrationBuilder.DropTable(
                name: "EstadosReservaActividad");

            migrationBuilder.DropIndex(
                name: "IX_ReservasActividades_EstadoReservaActividadId",
                table: "ReservasActividades");

            migrationBuilder.DropColumn(
                name: "EstadoReservaActividadId",
                table: "ReservasActividades");
        }
    }
}
