using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ft.Consultorio.MsMedicalRecords.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MedicalRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChiefComplaint = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CurrentIllness = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    MedicalDiagnosis = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    PhysiotherapyDiagnosis = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    ShortGoals = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    MediumGoals = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    LongGoals = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    Observations = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    History_Hypertension = table.Column<bool>(type: "boolean", nullable: false),
                    History_Diabetes = table.Column<bool>(type: "boolean", nullable: false),
                    History_Cancer = table.Column<bool>(type: "boolean", nullable: false),
                    History_Pacemaker = table.Column<bool>(type: "boolean", nullable: false),
                    History_Pregnancy = table.Column<bool>(type: "boolean", nullable: false),
                    History_Surgeries = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    History_Fractures = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    History_Medications = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    History_Allergies = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    History_OtherHistory = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MedicalRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false),
                    Error = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Document = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    FullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Gender = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Phone = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Email = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Instagram = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Occupation = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Address = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    EmergencyContact = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false),
                    Reference = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicalRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PainScale = table.Column<int>(type: "integer", nullable: false),
                    Evolution = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    TreatmentPerformed = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    Recommendations = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: true),
                    NextAppointment = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Price = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Paid = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MedicalRecords_PatientId",
                table: "MedicalRecords",
                column: "PatientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_OccurredOnUtc",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "OccurredOnUtc" },
                filter: "\"ProcessedOnUtc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Patients_Document",
                table: "Patients",
                column: "Document",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PatientId",
                table: "Payments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SessionId",
                table: "Payments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessions_MedicalRecordId",
                table: "Sessions",
                column: "MedicalRecordId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MedicalRecords");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropTable(
                name: "Patients");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "Sessions");
        }
    }
}
