using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Traninig_Managment_system.DAL.Migrations
{
    /// <inheritdoc />
    public partial class Addpdfcolomtolesson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PdfUrl",
                table: "lessons",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PdfUrl",
                table: "lessons");
        }
    }
}
