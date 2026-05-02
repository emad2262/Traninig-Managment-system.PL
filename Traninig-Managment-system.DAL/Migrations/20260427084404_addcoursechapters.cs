using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traninig_Managment_system.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addcoursechapters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChapterId",
                table: "lessons",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ChapterId",
                table: "Exams",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CourseChapters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseChapters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseChapters_courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_lessons_ChapterId",
                table: "lessons",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_Exams_ChapterId",
                table: "Exams",
                column: "ChapterId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseChapters_CourseId",
                table: "CourseChapters",
                column: "CourseId");

            migrationBuilder.AddForeignKey(
                name: "FK_Exams_CourseChapters_ChapterId",
                table: "Exams",
                column: "ChapterId",
                principalTable: "CourseChapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_lessons_CourseChapters_ChapterId",
                table: "lessons",
                column: "ChapterId",
                principalTable: "CourseChapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Exams_CourseChapters_ChapterId",
                table: "Exams");

            migrationBuilder.DropForeignKey(
                name: "FK_lessons_CourseChapters_ChapterId",
                table: "lessons");

            migrationBuilder.DropTable(
                name: "CourseChapters");

            migrationBuilder.DropIndex(
                name: "IX_lessons_ChapterId",
                table: "lessons");

            migrationBuilder.DropIndex(
                name: "IX_Exams_ChapterId",
                table: "Exams");

            migrationBuilder.DropColumn(
                name: "ChapterId",
                table: "lessons");

            migrationBuilder.DropColumn(
                name: "ChapterId",
                table: "Exams");
        }
    }
}
