using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Guarderia.Migrations
{
    /// <inheritdoc />
    public partial class Ultima : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoServicio",
                table: "MovimientosServicios",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCompletado",
                table: "MovimientosServicios",
                type: "datetime2",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Cuidadores",
                keyColumn: "IdCuidador",
                keyValue: 1,
                column: "FechaContratacion",
                value: new DateTime(2024, 12, 6, 18, 21, 59, 900, DateTimeKind.Local).AddTicks(8088));

            migrationBuilder.UpdateData(
                table: "Cuidadores",
                keyColumn: "IdCuidador",
                keyValue: 2,
                column: "FechaContratacion",
                value: new DateTime(2025, 6, 6, 18, 21, 59, 900, DateTimeKind.Local).AddTicks(8118));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 1,
                column: "FechaRegistro",
                value: new DateTime(2025, 12, 6, 18, 21, 59, 900, DateTimeKind.Local).AddTicks(7995));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 2,
                column: "FechaRegistro",
                value: new DateTime(2025, 12, 6, 18, 21, 59, 900, DateTimeKind.Local).AddTicks(8016));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstadoServicio",
                table: "MovimientosServicios");

            migrationBuilder.DropColumn(
                name: "FechaCompletado",
                table: "MovimientosServicios");

            migrationBuilder.UpdateData(
                table: "Cuidadores",
                keyColumn: "IdCuidador",
                keyValue: 1,
                column: "FechaContratacion",
                value: new DateTime(2024, 12, 6, 13, 44, 38, 799, DateTimeKind.Local).AddTicks(4648));

            migrationBuilder.UpdateData(
                table: "Cuidadores",
                keyColumn: "IdCuidador",
                keyValue: 2,
                column: "FechaContratacion",
                value: new DateTime(2025, 6, 6, 13, 44, 38, 799, DateTimeKind.Local).AddTicks(4678));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 1,
                column: "FechaRegistro",
                value: new DateTime(2025, 12, 6, 13, 44, 38, 799, DateTimeKind.Local).AddTicks(4554));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 2,
                column: "FechaRegistro",
                value: new DateTime(2025, 12, 6, 13, 44, 38, 799, DateTimeKind.Local).AddTicks(4575));
        }
    }
}
