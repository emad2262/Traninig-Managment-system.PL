using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traninig_Managment_system.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeLessonTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmployeeLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    IsCompleted = table.Column<bool>(type: "bit", nullable: false),
                    WatchedPercentage = table.Column<double>(type: "float", nullable: false),
                    LastWatchedSecond = table.Column<double>(type: "float", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeLessons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeLessons_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeLessons_lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "lessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLessons_EmployeeId_LessonId",
                table: "EmployeeLessons",
                columns: new[] { "EmployeeId", "LessonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeLessons_LessonId",
                table: "EmployeeLessons",
                column: "LessonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeLessons");
        }
    }
}
