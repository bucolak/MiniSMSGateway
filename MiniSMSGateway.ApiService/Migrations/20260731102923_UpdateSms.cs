using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MiniSMSGateway.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSms : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "id",
                table: "Messages",
                newName: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Messages",
                newName: "id");
        }
    }
}
