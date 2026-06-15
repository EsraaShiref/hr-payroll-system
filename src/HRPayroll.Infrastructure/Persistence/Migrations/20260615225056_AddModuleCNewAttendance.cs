using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HRPayroll.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddModuleCNewAttendance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AttendanceRecords");

            migrationBuilder.AddColumn<Guid>(
                name: "ShiftId",
                table: "Employees",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DefaultShiftId",
                table: "Departments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AttendanceDailySummaries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShiftName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ScheduledStart = table.Column<TimeOnly>(type: "time", nullable: false),
                    ScheduledEnd = table.Column<TimeOnly>(type: "time", nullable: false),
                    GracePeriodMinutes = table.Column<int>(type: "int", nullable: false),
                    LateThresholdMinutes = table.Column<int>(type: "int", nullable: false),
                    EarlyDepartureThresholdMinutes = table.Column<int>(type: "int", nullable: false),
                    OvertimeThresholdMinutes = table.Column<int>(type: "int", nullable: false),
                    MinimumWorkMinutesForPresence = table.Column<int>(type: "int", nullable: false),
                    FirstPunchIn = table.Column<TimeOnly>(type: "time", nullable: true),
                    LastPunchOut = table.Column<TimeOnly>(type: "time", nullable: true),
                    TotalBreakMinutes = table.Column<int>(type: "int", nullable: false),
                    TotalWorkedMinutes = table.Column<int>(type: "int", nullable: false),
                    LateMinutes = table.Column<int>(type: "int", nullable: false),
                    EarlyDepartureMinutes = table.Column<int>(type: "int", nullable: false),
                    OvertimeMinutes = table.Column<int>(type: "int", nullable: false),
                    IsUnexcusedAbsence = table.Column<bool>(type: "bit", nullable: false),
                    IsOnLeave = table.Column<bool>(type: "bit", nullable: false),
                    LeaveRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsHoliday = table.Column<bool>(type: "bit", nullable: false),
                    HolidayId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OverridePunchIn = table.Column<TimeOnly>(type: "time", nullable: true),
                    OverridePunchOut = table.Column<TimeOnly>(type: "time", nullable: true),
                    OverrideReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CalculatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CalculatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceDailySummaries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceDailySummaries_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AttendancePunches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TimestampUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsProcessed = table.Column<bool>(type: "bit", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendancePunches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendancePunches_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    IsRecurringYearly = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    GracePeriodMinutes = table.Column<int>(type: "int", nullable: false),
                    LateThresholdMinutes = table.Column<int>(type: "int", nullable: false),
                    EarlyDepartureThresholdMinutes = table.Column<int>(type: "int", nullable: false),
                    OvertimeThresholdMinutes = table.Column<int>(type: "int", nullable: false),
                    MinimumWorkMinutesForPresence = table.Column<int>(type: "int", nullable: false),
                    MaxBreakMinutes = table.Column<int>(type: "int", nullable: false),
                    WorkingDays = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Employees_ShiftId",
                table: "Employees",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_Departments_DefaultShiftId",
                table: "Departments",
                column: "DefaultShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceDailySummaries_Date",
                table: "AttendanceDailySummaries",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceDailySummaries_EmployeeId_Date",
                table: "AttendanceDailySummaries",
                columns: new[] { "EmployeeId", "Date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AttendancePunches_IsProcessed",
                table: "AttendancePunches",
                column: "IsProcessed",
                filter: "[IsProcessed] = 0");

            migrationBuilder.CreateIndex(
                name: "IX_AttendancePunches_Timestamp",
                table: "AttendancePunches",
                column: "TimestampUtc");

            migrationBuilder.CreateIndex(
                name: "IX_AttendancePunches_Unique",
                table: "AttendancePunches",
                columns: new[] { "EmployeeId", "TimestampUtc", "Type", "DeviceId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_Date",
                table: "Holidays",
                column: "Date",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Departments_Shifts_DefaultShiftId",
                table: "Departments",
                column: "DefaultShiftId",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Employees_Shifts_ShiftId",
                table: "Employees",
                column: "ShiftId",
                principalTable: "Shifts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Departments_Shifts_DefaultShiftId",
                table: "Departments");

            migrationBuilder.DropForeignKey(
                name: "FK_Employees_Shifts_ShiftId",
                table: "Employees");

            migrationBuilder.DropTable(
                name: "AttendanceDailySummaries");

            migrationBuilder.DropTable(
                name: "AttendancePunches");

            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropIndex(
                name: "IX_Employees_ShiftId",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Departments_DefaultShiftId",
                table: "Departments");

            migrationBuilder.DropColumn(
                name: "ShiftId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "DefaultShiftId",
                table: "Departments");

            migrationBuilder.CreateTable(
                name: "AttendanceRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BreakDurationMinutes = table.Column<int>(type: "int", nullable: false),
                    ClockIn = table.Column<TimeOnly>(type: "time", nullable: true),
                    ClockOut = table.Column<TimeOnly>(type: "time", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateOnly>(type: "date", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AttendanceRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AttendanceRecords_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_Date",
                table: "AttendanceRecords",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_AttendanceRecords_EmployeeId_Date",
                table: "AttendanceRecords",
                columns: new[] { "EmployeeId", "Date" },
                unique: true);
        }
    }
}
