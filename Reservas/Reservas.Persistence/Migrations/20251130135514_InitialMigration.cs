using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Reservas.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ActividadesRecreativas",
                columns: table => new
                {
                    ActividadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HotelId = table.Column<int>(type: "int", nullable: false),
                    NombreActividad = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Categoria = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Ubicacion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HoraApertura = table.Column<TimeSpan>(type: "time", nullable: false),
                    HoraCierre = table.Column<TimeSpan>(type: "time", nullable: false),
                    CapacidadMaxima = table.Column<int>(type: "int", nullable: false),
                    PrecioPorPersona = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    RequiereReserva = table.Column<bool>(type: "bit", nullable: false),
                    DuracionMinutos = table.Column<int>(type: "int", nullable: true),
                    EstaActiva = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    ImagenURL = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ActividadesRecreativas", x => x.ActividadId);
                });

            migrationBuilder.CreateTable(
                name: "Reservas",
                columns: table => new
                {
                    ReservaId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HuespedId = table.Column<int>(type: "int", nullable: false),
                    HabitacionId = table.Column<int>(type: "int", nullable: false),
                    NumeroReserva = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    FechaCheckIn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaCheckOut = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumeroHuespedes = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    NumeroNinos = table.Column<int>(type: "int", nullable: false),
                    MontoTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    MontoPagado = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Pendiente"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckInRealizado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CheckOutRealizado = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreadoPor = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Observaciones = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservas", x => x.ReservaId);
                });

            migrationBuilder.CreateTable(
                name: "ReservasActividades",
                columns: table => new
                {
                    ReservaActividadId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActividadId = table.Column<int>(type: "int", nullable: false),
                    HuespedId = table.Column<int>(type: "int", nullable: false),
                    FechaReserva = table.Column<DateTime>(type: "date", nullable: false),
                    HoraReserva = table.Column<TimeSpan>(type: "time", nullable: false),
                    NumeroPersonas = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Estado = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false, defaultValue: "Confirmada"),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(getutcdate())"),
                    MontoTotal = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    NotasEspeciales = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RecordatorioEnviado = table.Column<bool>(type: "bit", nullable: false),
                    FechaRecordatorio = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReservasActividades", x => x.ReservaActividadId);
                    table.ForeignKey(
                        name: "FK_ReservasActividades_Actividades",
                        column: x => x.ActividadId,
                        principalTable: "ActividadesRecreativas",
                        principalColumn: "ActividadId");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_Estado_Fechas",
                table: "Reservas",
                columns: new[] { "Estado", "FechaCheckIn", "FechaCheckOut" });

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_HabitacionId",
                table: "Reservas",
                column: "HabitacionId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_HuespedId",
                table: "Reservas",
                column: "HuespedId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservas_NumeroReserva",
                table: "Reservas",
                column: "NumeroReserva");

            migrationBuilder.CreateIndex(
                name: "UQ_Reservas_NumeroReserva",
                table: "Reservas",
                column: "NumeroReserva",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ReservasActividades_ActividadId",
                table: "ReservasActividades",
                column: "ActividadId");

            migrationBuilder.CreateIndex(
                name: "IX_ReservasActividades_Fecha",
                table: "ReservasActividades",
                columns: new[] { "FechaReserva", "HoraReserva" });

            migrationBuilder.CreateIndex(
                name: "IX_ReservasActividades_HuespedId",
                table: "ReservasActividades",
                column: "HuespedId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reservas");

            migrationBuilder.DropTable(
                name: "ReservasActividades");

            migrationBuilder.DropTable(
                name: "ActividadesRecreativas");
        }
    }
}
