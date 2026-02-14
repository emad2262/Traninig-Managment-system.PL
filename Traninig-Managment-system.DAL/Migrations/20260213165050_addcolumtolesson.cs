using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traninig_Managment_system.DAL.Migrations
{
    /// <inheritdoc />
    public partial class addcolumtolesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Duration",
                table: "lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Duration",
                table: "lessons");
        }
    }
}
