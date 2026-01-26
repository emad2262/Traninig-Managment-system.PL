using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Traninig_Managment_system.DAL.Migrations
{
    /// <inheritdoc />
    public partial class plandata : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "plans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.InsertData(
                table: "plans",
                columns: new[] { "Id", "CreatedByAdminId", "DurationInDays", "IsActive", "MaxCourses", "MaxEmployees", "Name", "Price", "Type" },
                values: new object[,]
                {
                    { 1, null, 30, true, 5, 20, "Basic", 199.0, 0 },
                    { 2, null, 30, true, 15, 50, "Pro", 399.0, 0 },
                    { 3, null, 30, true, 50, 200, "Premium", 699.0, 0 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "plans",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "plans",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "plans",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DropColumn(
                name: "Type",
                table: "plans");
        }
    }
}
