using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModificadoPorToReserva : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "Reservas",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ModificadoPor",
                table: "Reservas");
        }
    }
}
