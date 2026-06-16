using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRPayroll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleESelfServiceFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmailChangePending",
                table: "Employees",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PendingNewEmail",
                table: "Employees",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisputeReason",
                table: "AttendanceDailySummaries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisputeResolution",
                table: "AttendanceDailySummaries",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DisputeResolvedAt",
                table: "AttendanceDailySummaries",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisputeResolvedBy",
                table: "AttendanceDailySummaries",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DisputeStatus",
                table: "AttendanceDailySummaries",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "DisputedPunchIn",
                table: "AttendanceDailySummaries",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeOnly>(
                name: "DisputedPunchOut",
                table: "AttendanceDailySummaries",
                type: "time",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmailChangePending",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PendingNewEmail",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DisputeReason",
                table: "AttendanceDailySummaries");

            migrationBuilder.DropColumn(
                name: "DisputeResolution",
                table: "AttendanceDailySummaries");

            migrationBuilder.DropColumn(
                name: "DisputeResolvedAt",
                table: "AttendanceDailySummaries");

            migrationBuilder.DropColumn(
                name: "DisputeResolvedBy",
                table: "AttendanceDailySummaries");

            migrationBuilder.DropColumn(
                name: "DisputeStatus",
                table: "AttendanceDailySummaries");

            migrationBuilder.DropColumn(
                name: "DisputedPunchIn",
                table: "AttendanceDailySummaries");

            migrationBuilder.DropColumn(
                name: "DisputedPunchOut",
                table: "AttendanceDailySummaries");
        }
    }
}
