using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traninig_Managment_system.DAL.Migrations
{
    /// <inheritdoc />
    public partial class AddCompanyCertificateWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "CompanyNotifications",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReadAt",
                table: "CompanyNotifications",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReferenceId",
                table: "CompanyNotifications",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceType",
                table: "CompanyNotifications",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "EmployeeCertificates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    CourseId = table.Column<int>(type: "int", nullable: false),
                    CertificateNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IssuedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CompanyNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    FinalScore = table.Column<double>(type: "float", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeCertificates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeCertificates_courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmployeeCertificates_employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCertificates_CompanyId_Status",
                table: "EmployeeCertificates",
                columns: new[] { "CompanyId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCertificates_CourseId",
                table: "EmployeeCertificates",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeCertificates_EmployeeId_CourseId",
                table: "EmployeeCertificates",
                columns: new[] { "EmployeeId", "CourseId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmployeeCertificates");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "CompanyNotifications");

            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "CompanyNotifications");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "CompanyNotifications");

            migrationBuilder.DropColumn(
                name: "ReferenceType",
                table: "CompanyNotifications");
        }
    }
}
