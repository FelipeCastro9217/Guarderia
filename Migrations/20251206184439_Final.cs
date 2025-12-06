using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Guarderia.Migrations
{
    /// <inheritdoc />
    public partial class Final : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "IdCuidador",
                table: "MovimientosServicios",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosServicios_IdCuidador",
                table: "MovimientosServicios",
                column: "IdCuidador");

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosServicios_Cuidadores_IdCuidador",
                table: "MovimientosServicios",
                column: "IdCuidador",
                principalTable: "Cuidadores",
                principalColumn: "IdCuidador",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosServicios_Cuidadores_IdCuidador",
                table: "MovimientosServicios");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosServicios_IdCuidador",
                table: "MovimientosServicios");

            migrationBuilder.DropColumn(
                name: "IdCuidador",
                table: "MovimientosServicios");

            migrationBuilder.UpdateData(
                table: "Cuidadores",
                keyColumn: "IdCuidador",
                keyValue: 1,
                column: "FechaContratacion",
                value: new DateTime(2024, 12, 5, 21, 55, 57, 831, DateTimeKind.Local).AddTicks(8702));

            migrationBuilder.UpdateData(
                table: "Cuidadores",
                keyColumn: "IdCuidador",
                keyValue: 2,
                column: "FechaContratacion",
                value: new DateTime(2025, 6, 5, 21, 55, 57, 831, DateTimeKind.Local).AddTicks(8734));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 1,
                column: "FechaRegistro",
                value: new DateTime(2025, 12, 5, 21, 55, 57, 831, DateTimeKind.Local).AddTicks(8608));

            migrationBuilder.UpdateData(
                table: "Usuarios",
                keyColumn: "IdUsuario",
                keyValue: 2,
                column: "FechaRegistro",
                value: new DateTime(2025, 12, 5, 21, 55, 57, 831, DateTimeKind.Local).AddTicks(8629));
        }
    }
}
