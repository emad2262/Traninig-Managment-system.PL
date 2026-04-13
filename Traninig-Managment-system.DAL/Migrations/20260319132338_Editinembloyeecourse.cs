using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traninig_Managment_system.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Editinembloyeecourse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "EmployeeCourses");

            migrationBuilder.DropColumn(
                name: "PointsEarned",
                table: "EmployeeCourses");

            migrationBuilder.AlterColumn<double>(
                name: "Progress",
                table: "EmployeeCourses",
                type: "float",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<double>(
                name: "FinalScore",
                table: "EmployeeCourses",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastAccessedAt",
                table: "EmployeeCourses",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "EmployeeCourses",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FinalScore",
                table: "EmployeeCourses");

            migrationBuilder.DropColumn(
                name: "LastAccessedAt",
                table: "EmployeeCourses");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "EmployeeCourses");

            migrationBuilder.AlterColumn<bool>(
                name: "Progress",
                table: "EmployeeCourses",
                type: "bit",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "EmployeeCourses",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<double>(
                name: "PointsEarned",
                table: "EmployeeCourses",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
