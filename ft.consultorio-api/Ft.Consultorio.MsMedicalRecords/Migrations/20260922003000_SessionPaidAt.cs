using System;
using Ft.Consultorio.MsMedicalRecords.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ft.Consultorio.MsMedicalRecords.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260922003000_SessionPaidAt")]
    public class SessionPaidAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "PaidAt",
                table: "Sessions",
                type: "timestamp with time zone",
                nullable: true);

            // Las sesiones que ya estaban pagadas cuentan en el mes de la cita.
            migrationBuilder.Sql(
                """
                UPDATE "Sessions" SET "PaidAt" = "Date" WHERE "Paid" = true AND "PaidAt" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaidAt",
                table: "Sessions");
        }
    }
}
