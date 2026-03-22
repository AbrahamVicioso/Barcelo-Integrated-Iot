using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Reservas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEstadosReservaTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create the lookup table
            migrationBuilder.CreateTable(
                name: "EstadosReserva",
                columns: table => new
                {
                    EstadoReservaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EstadosReserva", x => x.EstadoReservaId);
                });

            // 2. Add the new FK column (nullable initially to allow data migration)
            migrationBuilder.AddColumn<int>(
                name: "EstadoReservaId",
                table: "Reservas",
                type: "int",
                nullable: false,
                defaultValue: 1);

            // 4. Populate the new FK column from the old Estado string
            migrationBuilder.Sql(@"
                UPDATE [dbo].[Reservas]
                SET EstadoReservaId = CASE Estado
                    WHEN 'Pendiente' THEN 1
                    WHEN 'Activa'    THEN 2
                    WHEN 'CheckIn'   THEN 3
                    WHEN 'CheckOut'  THEN 4
                    WHEN 'Cancelada' THEN 5
                    ELSE 1
                END
            ");

            // 5. Drop old index that referenced Estado string column
            migrationBuilder.DropIndex(
                name: "IX_Reservas_Estado_Fechas",
                table: "Reservas");

            // 6. Drop the old Estado string column
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Reservas");

            // 7. Add FK constraint
            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_EstadosReserva",
                table: "Reservas",
                column: "EstadoReservaId",
                principalTable: "EstadosReserva",
                principalColumn: "EstadoReservaId",
                onDelete: ReferentialAction.Restrict);

            // 8. Create new indexes for the FK column
            migrationBuilder.CreateIndex(
                name: "IX_Reservas_Estado_Fechas",
                table: "Reservas",
                columns: new[] { "EstadoReservaId", "FechaCheckIn", "FechaCheckOut" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_EstadoReservaId",
                table: "Reservas",
                column: "EstadoReservaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_EstadosReserva",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_Estado_Fechas",
                table: "Reservas");

            migrationBuilder.DropIndex(
                name: "IX_Reservas_EstadoReservaId",
                table: "Reservas");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Reservas",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Pendiente");

            migrationBuilder.Sql(@"
                UPDATE [dbo].[Reservas]
                SET Estado = CASE EstadoReservaId
                    WHEN 1 THEN 'Pendiente'
                    WHEN 2 THEN 'Activa'
                    WHEN 3 THEN 'CheckIn'
                    WHEN 4 THEN 'CheckOut'
                    WHEN 5 THEN 'Cancelada'
                    ELSE 'Pendiente'
                END
            ");

            migrationBuilder.DropColumn(
                name: "EstadoReservaId",
                table: "Reservas");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_Estado_Fechas",
                table: "Reservas",
                columns: new[] { "Estado", "FechaCheckIn", "FechaCheckOut" });

            migrationBuilder.DropTable(
                name: "EstadosReserva");
        }
    }
}
