using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Guarderia.Migrations
{
    /// <inheritdoc />
    public partial class Completa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Cuidadores",
                columns: table => new
                {
                    IdCuidador = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Apellido = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Telefono = table.Column<string>(type: "nvarchar(15)", maxLength: 15, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Especialidad = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    FechaContratacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cuidadores", x => x.IdCuidador);
                });

            migrationBuilder.InsertData(
                table: "Cuidadores",
                columns: new[] { "IdCuidador", "Activo", "Apellido", "Email", "Especialidad", "FechaContratacion", "Nombre", "Telefono" },
                values: new object[,]
                {
                    { 1, true, "Rodríguez", "carlos.rodriguez@guarderia.com", "Perros grandes", new DateTime(2024, 12, 5, 20, 23, 41, 55, DateTimeKind.Local).AddTicks(2512), "Carlos", "3001234567" },
                    { 2, true, "López", "maria.lopez@guarderia.com", "Perros pequeños", new DateTime(2025, 6, 5, 20, 23, 41, 55, DateTimeKind.Local).AddTicks(2545), "María", "3009876543" }
                });

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 1,
                column: "FechaRegistro",
                value: new DateTime(2025, 12, 5, 20, 23, 41, 55, DateTimeKind.Local).AddTicks(2413));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 2,
                column: "FechaRegistro",
                value: new DateTime(2025, 12, 5, 20, 23, 41, 55, DateTimeKind.Local).AddTicks(2435));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Cuidadores");

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 1,
                column: "FechaRegistro",
                value: new DateTime(2025, 12, 5, 19, 5, 56, 993, DateTimeKind.Local).AddTicks(4978));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 2,
                column: "FechaRegistro",
                value: new DateTime(2025, 12, 5, 19, 5, 56, 993, DateTimeKind.Local).AddTicks(4999));
        }
    }
}
