using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchoolManagement.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAcademicEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sections_ClassId",
                table: "Sections");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Sections",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Sections",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<Guid>(
                name: "ClassTeacherId",
                table: "Sections",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentStrength",
                table: "Sections",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "RoomNumber",
                table: "Sections",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            // ===== FIXED: Drop and Add RowVersion columns =====

            // PayrollRecords
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PayrollRecords");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PayrollRecords",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            // Notifications
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Notifications");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Notifications",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            // Designations
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Designations");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Designations",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            // Deductions
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Deductions");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Deductions",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            // Allowances
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Allowances");

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Allowances",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            // Classes - Drop RowVersion first
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Classes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Classes",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Classes",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Classes",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "AcademicYearId",
                table: "Classes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "Classes",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "Grade",
                table: "Classes",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Classes - Add RowVersion back
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Classes",
                type: "rowversion",
                rowVersion: true,
                nullable: false);

            // ===== DATA MIGRATION: Update empty Code values before creating unique index =====
            migrationBuilder.Sql(@"
                -- Generate unique codes for existing classes using CTE
                WITH NumberedClasses AS (
                    SELECT Id, 
                           'CLS-' + RIGHT('0000' + CAST(ROW_NUMBER() OVER (ORDER BY Id) AS NVARCHAR(10)), 4) AS NewCode
                    FROM Classes
                    WHERE Code = '' OR Code IS NULL
                )
                UPDATE Classes
                SET Code = nc.NewCode
                FROM Classes c
                INNER JOIN NumberedClasses nc ON c.Id = nc.Id;
            ");

            migrationBuilder.CreateTable(
                name: "SectionSubjects",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SubjectCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WeeklyPeriods = table.Column<int>(type: "int", nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SectionSubjects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SectionSubjects_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TimeTableEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SectionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TeacherId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PeriodNumber = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    CreatedIP = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimeTableEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TimeTableEntries_Sections_SectionId",
                        column: x => x.SectionId,
                        principalTable: "Sections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Sections_Class_Name",
                table: "Sections",
                columns: new[] { "ClassId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sections_ClassTeacher",
                table: "Sections",
                column: "ClassTeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_AcademicYear",
                table: "Classes",
                column: "AcademicYearId");

            migrationBuilder.CreateIndex(
                name: "IX_Classes_ClassCode",
                table: "Classes",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Classes_Grade",
                table: "Classes",
                column: "Grade");

            migrationBuilder.CreateIndex(
                name: "IX_SectionSubjects_Section_Subject",
                table: "SectionSubjects",
                columns: new[] { "SectionId", "SubjectId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SectionSubjects_Teacher",
                table: "SectionSubjects",
                column: "TeacherId");

            migrationBuilder.CreateIndex(
                name: "IX_TimeTable_Day_Period",
                table: "TimeTableEntries",
                columns: new[] { "DayOfWeek", "PeriodNumber" });

            migrationBuilder.CreateIndex(
                name: "IX_TimeTable_Section_Day_Period",
                table: "TimeTableEntries",
                columns: new[] { "SectionId", "DayOfWeek", "PeriodNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TimeTable_Teacher",
                table: "TimeTableEntries",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SectionSubjects");

            migrationBuilder.DropTable(
                name: "TimeTableEntries");

            migrationBuilder.DropIndex(
                name: "IX_Sections_Class_Name",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Sections_ClassTeacher",
                table: "Sections");

            migrationBuilder.DropIndex(
                name: "IX_Classes_AcademicYear",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_ClassCode",
                table: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Classes_Grade",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "ClassTeacherId",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "CurrentStrength",
                table: "Sections");

            migrationBuilder.DropColumn(
                name: "RoomNumber",
                table: "Sections");

            // Drop new RowVersion columns
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "PayrollRecords");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Designations");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Deductions");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Allowances");

            migrationBuilder.DropColumn(
                name: "AcademicYearId",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "Classes");

            migrationBuilder.DropColumn(
                name: "Grade",
                table: "Classes");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Sections",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Sections",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            // Add back old RowVersion columns
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "PayrollRecords",
                type: "varbinary(max)",
                nullable: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Notifications",
                type: "varbinary(max)",
                nullable: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Designations",
                type: "varbinary(max)",
                nullable: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Deductions",
                type: "varbinary(max)",
                nullable: false);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Allowances",
                type: "varbinary(max)",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Classes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "IsActive",
                table: "Classes",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Classes",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Classes",
                type: "varbinary(max)",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_Sections_ClassId",
                table: "Sections",
                column: "ClassId");
        }
    }
}