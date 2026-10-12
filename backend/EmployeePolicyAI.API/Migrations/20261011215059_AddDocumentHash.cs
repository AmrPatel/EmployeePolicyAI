using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EmployeePolicyAI.API.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentHash : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DocumentHash",
                table: "DocumentChunks",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DocumentHash",
                table: "DocumentChunks");
        }
    }
}
